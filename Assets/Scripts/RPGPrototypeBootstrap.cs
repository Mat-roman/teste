using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RPGPrototype
{
    /// <summary>
    /// Runtime bootstrap that creates a playable RPG prototype in an empty Unity scene.
    /// </summary>
    public sealed class RPGPrototypeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBootstrap()
        {
            if (FindObjectOfType<RPGPrototypeBootstrap>() != null) return;
            var go = new GameObject("RPGPrototypeBootstrap");
            DontDestroyOnLoad(go);
            go.AddComponent<RPGPrototypeBootstrap>();
        }

        private void Start()
        {
            GameManager.Ensure();
            InventorySystem.Ensure();
            QuestSystem.Ensure();
            SaveSystem.Ensure();

            World.BuildAreas();
            var player = World.BuildPlayer();
            World.BuildEnemies();
            World.BuildNpcs();
            World.BuildInteractables();
            UIRuntime.Ensure(player);
        }
    }

    public enum EnemyType { Goblin, Orc, Skeleton, Dragon, Bandit }
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum ItemType { Consumable, Helmet, Chest, Legs, Boots, Hands, Weapon, Shield }
    public enum QuestType { Kill, Collect, Explore, Talk }

    [Serializable]
    public sealed class ItemData
    {
        public string Name;
        public ItemType Type;
        public ItemRarity Rarity;
        public int Damage;
        public int Armor;
        public int HP;
        public int Mana;
    }

    [Serializable]
    public sealed class QuestData
    {
        public string Name;
        public QuestType Type;
        public int Target;
        public int Progress;
        public bool Completed;
        public int RewardXp;
        public int RewardGold;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int Level;
        public int XP;
        public int Gold;
        public float HP;
        public float Mana;
        public Vector3 Position;
    }

    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public int EnemiesKilled;
        public int QuestsCompleted;
        public float PlayTime;

        public static void Ensure()
        {
            if (Instance != null) return;
            var go = new GameObject("GameManager");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<GameManager>();
            go.AddComponent<DayNightWeather>();
        }

        private void Update() => PlayTime += Time.deltaTime;
    }

    public sealed class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }
        public static void Ensure()
        {
            if (Instance != null) return;
            var go = new GameObject("SaveSystem");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<SaveSystem>();
        }

        public void Save(int slot, PlayerRuntime player)
        {
            var data = new SaveData
            {
                Level = player.Level,
                XP = player.XP,
                Gold = player.Gold,
                HP = player.CurrentHP,
                Mana = player.CurrentMana,
                Position = player.transform.position
            };
            PlayerPrefs.SetString($"RPG_SLOT_{slot}", JsonUtility.ToJson(data));
        }

        public void Load(int slot, PlayerRuntime player)
        {
            var key = $"RPG_SLOT_{slot}";
            if (!PlayerPrefs.HasKey(key)) return;
            var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(key));
            player.Level = data.Level;
            player.XP = data.XP;
            player.Gold = data.Gold;
            player.CurrentHP = data.HP;
            player.CurrentMana = data.Mana;
            player.transform.position = data.Position;
        }
    }

    public sealed class PlayerRuntime : MonoBehaviour
    {
        public int Level = 1;
        public int XP;
        public int Gold;
        public int Strength = 5;
        public int Dexterity = 5;
        public int Intelligence = 5;
        public int Constitution = 5;
        public int Luck = 5;
        public float CurrentHP = 100f;
        public float CurrentMana = 60f;

        public float MaxHP => 100f + Constitution * 10f;
        public float MaxMana => 60f + Intelligence * 8f;

        public void GainXP(int amount)
        {
            XP += amount;
            while (XP >= NeededXP())
            {
                XP -= NeededXP();
                Level++;
                Strength++;
                Dexterity++;
                Intelligence++;
                Constitution++;
                Luck++;
                CurrentHP = MaxHP;
                CurrentMana = MaxMana;
            }
        }

        public int NeededXP() => 100 + (Level - 1) * 50;
    }

    public sealed class PlayerController : MonoBehaviour
    {
        private CharacterController _controller;
        private PlayerRuntime _player;
        private Camera _cam;
        private float _yaw;
        private float _pitch;
        private float _yVelocity;
        private float _nextAttack;
        private float _nextMagic;
        private bool _thirdPerson;
        private bool _blocking;
        private float _dodgeUntil;

        private void Awake()
        {
            _controller = gameObject.AddComponent<CharacterController>();
            _player = GetComponent<PlayerRuntime>();
            _cam = Camera.main;
            if (_cam == null)
            {
                var c = new GameObject("Main Camera");
                c.tag = "MainCamera";
                _cam = c.AddComponent<Camera>();
            }
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            var run = ReadRun();
            var speed = run ? 7f : 4.5f;
            var move = ReadMove();
            var worldMove = transform.TransformDirection(move) * speed;

            var look = ReadLook();
            _yaw += look.x * 4f;
            _pitch = Mathf.Clamp(_pitch - look.y * 4f, -80f, 80f);
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

            if (_controller.isGrounded)
            {
                _yVelocity = -1f;
                if (ReadJump()) _yVelocity = 6f;
            }
            _yVelocity += Physics.gravity.y * Time.deltaTime;
            _controller.Move((worldMove + Vector3.up * _yVelocity) * Time.deltaTime);

            _blocking = ReadBlock();
            if (ReadDodge()) _dodgeUntil = Time.time + 0.25f;
            if (ReadToggleCamera()) _thirdPerson = !_thirdPerson;
            _cam.transform.position = _thirdPerson
                ? transform.position - transform.forward * 3f + Vector3.up * 1.8f
                : transform.position + Vector3.up * 1.6f;
            _cam.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            if (ReadAttack() && Time.time >= _nextAttack)
            {
                _nextAttack = Time.time + 0.5f;
                foreach (var hit in Physics.OverlapSphere(transform.position + transform.forward * 1.5f, 1.8f))
                {
                    if (hit.TryGetComponent<EnemyRuntime>(out var enemy))
                    {
                        var crit = UnityEngine.Random.value < (0.05f + _player.Luck * 0.005f);
                        enemy.TakeDamage((10f + _player.Strength * 1.5f) * (crit ? 1.7f : 1f));
                    }
                }
            }

            if (ReadMagic() && Time.time >= _nextMagic && _player.CurrentMana >= 8f)
            {
                _player.CurrentMana -= 8f;
                _nextMagic = Time.time + 1.2f;
                var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projectile.transform.position = transform.position + Vector3.up * 1.2f + transform.forward;
                projectile.transform.localScale = Vector3.one * 0.25f;
                projectile.GetComponent<Collider>().isTrigger = true;
                var rb = projectile.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.velocity = transform.forward * 20f;
                projectile.AddComponent<Projectile>().Damage = 14f + _player.Intelligence * 1.2f;
                Destroy(projectile, 3f);
            }

            if (_player.CurrentHP <= 0f)
            {
                _player.transform.position = Vector3.up * 2f;
                _player.CurrentHP = _player.MaxHP;
                _player.CurrentMana = _player.MaxMana;
            }
        }

        public void ReceiveDamage(float amount)
        {
            if (Time.time <= _dodgeUntil) return;
            if (_blocking) amount *= 0.5f;
            _player.CurrentHP -= amount;
        }

        private static Vector3 ReadMove()
        {
#if ENABLE_INPUT_SYSTEM
            var x = (Keyboard.current?.aKey.isPressed == true ? -1f : 0f) + (Keyboard.current?.dKey.isPressed == true ? 1f : 0f);
            var z = (Keyboard.current?.sKey.isPressed == true ? -1f : 0f) + (Keyboard.current?.wKey.isPressed == true ? 1f : 0f);
            return new Vector3(x, 0f, z);
#else
            return new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
#endif
        }

        private static Vector2 ReadLook()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.delta.ReadValue() * 0.04f : Vector2.zero;
#else
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
        }

        private static bool ReadRun()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current?.leftShiftKey.isPressed == true;
#else
            return Input.GetKey(KeyCode.LeftShift);
#endif
        }

        private static bool ReadJump()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current?.spaceKey.wasPressedThisFrame == true;
#else
            return Input.GetKeyDown(KeyCode.Space);
#endif
        }

        private static bool ReadToggleCamera()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current?.vKey.wasPressedThisFrame == true;
#else
            return Input.GetKeyDown(KeyCode.V);
#endif
        }

        private static bool ReadAttack()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current?.leftButton.wasPressedThisFrame == true;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        private static bool ReadMagic()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current?.rightButton.wasPressedThisFrame == true;
#else
            return Input.GetMouseButtonDown(1);
#endif
        }

        private static bool ReadBlock()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current?.qKey.isPressed == true;
#else
            return Input.GetKey(KeyCode.Q);
#endif
        }

        private static bool ReadDodge()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current?.leftCtrlKey.wasPressedThisFrame == true;
#else
            return Input.GetKeyDown(KeyCode.LeftControl);
#endif
        }
    }

    public sealed class Projectile : MonoBehaviour
    {
        public float Damage;
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<EnemyRuntime>(out var enemy))
            {
                enemy.TakeDamage(Damage);
                Destroy(gameObject);
            }
        }
    }

    public sealed class EnemyRuntime : MonoBehaviour
    {
        public EnemyType Type;
        public float HP;
        public float Damage;
        public float Speed;
        private PlayerRuntime _target;
        private float _nextAttack;

        public static void Spawn(EnemyType type, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.transform.position = position;
            var e = go.AddComponent<EnemyRuntime>();
            e.Type = type;
            e.Setup();
        }

        private void Setup()
        {
            switch (Type)
            {
                case EnemyType.Goblin: HP = 40f; Damage = 7f; Speed = 2.8f; break;
                case EnemyType.Orc: HP = 85f; Damage = 12f; Speed = 2.2f; break;
                case EnemyType.Skeleton: HP = 60f; Damage = 9f; Speed = 2.5f; break;
                case EnemyType.Dragon: HP = 220f; Damage = 20f; Speed = 3.1f; transform.localScale = Vector3.one * 2.4f; break;
                default: HP = 90f; Damage = 11f; Speed = 3.1f; break;
            }
        }

        private void Update()
        {
            if (_target == null) _target = FindObjectOfType<PlayerRuntime>();
            if (_target == null) return;
            var dist = Vector3.Distance(transform.position, _target.transform.position);
            if (dist > 12f) return;

            transform.LookAt(_target.transform.position);
            if (dist > 1.9f)
            {
                transform.position += transform.forward * (Speed * Time.deltaTime);
                return;
            }

            if (Time.time < _nextAttack) return;
            _nextAttack = Time.time + 1.1f;
            if (_target.TryGetComponent<PlayerController>(out var controller))
            {
                controller.ReceiveDamage(Damage);
            }
            else
            {
                _target.CurrentHP -= Damage;
            }
        }

        public void TakeDamage(float amount)
        {
            HP -= amount;
            if (HP > 0f) return;
            if (FindObjectOfType<PlayerRuntime>() is { } player)
            {
                player.GainXP(20);
                player.Gold += 5;
                QuestSystem.Instance.AddProgress(QuestType.Kill, 1);
            }
            GameManager.Instance.EnemiesKilled++;
            if (UnityEngine.Random.value < 0.45f) InventorySystem.Instance.Add(InventorySystem.Instance.RandomItem());
            Destroy(gameObject);
        }
    }

    public sealed class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }
        public List<ItemData> Items { get; } = new();
        private readonly List<ItemData> _catalog = new();

        public static void Ensure()
        {
            if (Instance != null) return;
            var go = new GameObject("InventorySystem");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<InventorySystem>();
            Instance.SeedCatalog();
        }

        private void SeedCatalog()
        {
            for (var i = 1; i <= 30; i++)
                _catalog.Add(new ItemData { Name = $"Equipment {i}", Type = (ItemType)((i % 7) + 1), Rarity = (ItemRarity)Mathf.Min(4, i / 7), Damage = i % 9, Armor = i % 11, HP = i % 20, Mana = i % 15 });
            for (var i = 1; i <= 50; i++)
                _catalog.Add(new ItemData { Name = $"Consumable {i}", Type = ItemType.Consumable, Rarity = (ItemRarity)Mathf.Min(4, i / 12), HP = i % 25, Mana = i % 18 });
        }

        public ItemData RandomItem() => _catalog[UnityEngine.Random.Range(0, _catalog.Count)];
        public void Add(ItemData item) { if (Items.Count < 24) Items.Add(item); }
    }

    public sealed class QuestSystem : MonoBehaviour
    {
        public static QuestSystem Instance { get; private set; }
        public List<QuestData> Quests { get; } = new();

        public static void Ensure()
        {
            if (Instance != null) return;
            var go = new GameObject("QuestSystem");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<QuestSystem>();
            for (var i = 1; i <= 15; i++)
                Instance.Quests.Add(new QuestData { Name = $"Quest Chain {((i - 1) / 3) + 1} Step {((i - 1) % 3) + 1}", Type = (QuestType)(i % 4), Target = 2 + (i % 5), RewardXp = 50 + i * 10, RewardGold = 10 + i * 3 });
        }

        public void AddProgress(QuestType type, int amount)
        {
            var player = FindObjectOfType<PlayerRuntime>();
            foreach (var q in Quests)
            {
                if (q.Completed || q.Type != type) continue;
                q.Progress += amount;
                if (q.Progress < q.Target) return;
                q.Completed = true;
                GameManager.Instance.QuestsCompleted++;
                if (player != null) { player.GainXP(q.RewardXp); player.Gold += q.RewardGold; }
                return;
            }
        }
    }

    public sealed class UIRuntime : MonoBehaviour
    {
        private static UIRuntime _instance;
        private PlayerRuntime _player;
        private bool _inv;
        private bool _quests;
        private bool _pause;

        public static void Ensure(GameObject player)
        {
            if (_instance == null)
            {
                var go = new GameObject("UIRuntime");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<UIRuntime>();
            }
            _instance._player = player.GetComponent<PlayerRuntime>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I)) _inv = !_inv;
            if (Input.GetKeyDown(KeyCode.J)) _quests = !_quests;
            if (Input.GetKeyDown(KeyCode.Escape)) _pause = !_pause;
            Time.timeScale = _pause ? 0f : 1f;
        }

        private void OnGUI()
        {
            if (_player == null) return;
            GUI.Box(new Rect(12, 12, 270, 100), $"Lvl {_player.Level} Gold {_player.Gold}\nHP {_player.CurrentHP:0}/{_player.MaxHP:0}\nMana {_player.CurrentMana:0}/{_player.MaxMana:0}\nXP {_player.XP}/{_player.NeededXP()}");
            GUI.Label(new Rect(12, 118, 650, 20), "WASD mover | Shift correr | Espaço pular | Mouse atacar/magia | V câmera | I inventário | J quests");

            if (_inv)
            {
                GUI.Box(new Rect(12, 140, 420, 360), "Inventário (24 slots)");
                var y = 170;
                for (var i = 0; i < Mathf.Min(InventorySystem.Instance.Items.Count, 15); i++)
                {
                    var item = InventorySystem.Instance.Items[i];
                    GUI.Label(new Rect(20, y, 410, 18), $"{i + 1:00} {item.Name} [{item.Rarity}] D:{item.Damage} A:{item.Armor}");
                    y += 20;
                }
            }

            if (_quests)
            {
                GUI.Box(new Rect(450, 140, 430, 360), "Quest Log");
                var y = 170;
                foreach (var q in QuestSystem.Instance.Quests)
                {
                    GUI.Label(new Rect(460, y, 410, 18), $"{(q.Completed ? "[OK]" : "[..]")} {q.Name} ({q.Progress}/{q.Target})");
                    y += 20;
                    if (y > 480) break;
                }
            }

            if (!_pause) return;
            GUI.Box(new Rect(Screen.width / 2f - 170, Screen.height / 2f - 130, 340, 260), "Pause / Save-Load");
            if (GUI.Button(new Rect(Screen.width / 2f - 150, Screen.height / 2f - 95, 90, 24), "Save 1")) SaveSystem.Instance.Save(1, _player);
            if (GUI.Button(new Rect(Screen.width / 2f - 55, Screen.height / 2f - 95, 90, 24), "Save 2")) SaveSystem.Instance.Save(2, _player);
            if (GUI.Button(new Rect(Screen.width / 2f + 40, Screen.height / 2f - 95, 90, 24), "Save 3")) SaveSystem.Instance.Save(3, _player);
            if (GUI.Button(new Rect(Screen.width / 2f - 150, Screen.height / 2f - 62, 90, 24), "Load 1")) SaveSystem.Instance.Load(1, _player);
            if (GUI.Button(new Rect(Screen.width / 2f - 55, Screen.height / 2f - 62, 90, 24), "Load 2")) SaveSystem.Instance.Load(2, _player);
            if (GUI.Button(new Rect(Screen.width / 2f + 40, Screen.height / 2f - 62, 90, 24), "Load 3")) SaveSystem.Instance.Load(3, _player);
            GUI.Label(new Rect(Screen.width / 2f - 150, Screen.height / 2f - 20, 300, 40), $"Stats: Kills {GameManager.Instance.EnemiesKilled} | Quests {GameManager.Instance.QuestsCompleted}\nPlaytime {GameManager.Instance.PlayTime:0}s");
            if (GUI.Button(new Rect(Screen.width / 2f - 150, Screen.height / 2f + 40, 280, 28), "Resume")) _pause = false;
        }
    }

    public static class World
    {
        public static void BuildAreas()
        {
            CreateArea("Village", new Vector3(0f, 0f, 0f), Color.green);
            CreateArea("Forest", new Vector3(70f, 0f, 0f), new Color(0.2f, 0.45f, 0.15f));
            CreateArea("Dungeon", new Vector3(0f, 0f, 70f), Color.gray);
            CreateArea("Castle Entrance", new Vector3(70f, 0f, 70f), new Color(0.4f, 0.4f, 0.5f));
        }

        public static GameObject BuildPlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = Vector3.up * 2f;
            player.GetComponent<Renderer>().enabled = false;
            player.AddComponent<PlayerRuntime>();
            player.AddComponent<PlayerController>();
            return player;
        }

        public static void BuildEnemies()
        {
            EnemyRuntime.Spawn(EnemyType.Goblin, new Vector3(10f, 1f, 10f));
            EnemyRuntime.Spawn(EnemyType.Orc, new Vector3(20f, 1f, -8f));
            EnemyRuntime.Spawn(EnemyType.Skeleton, new Vector3(80f, 1f, 6f));
            EnemyRuntime.Spawn(EnemyType.Bandit, new Vector3(8f, 1f, 80f));
            EnemyRuntime.Spawn(EnemyType.Dragon, new Vector3(80f, 1f, 80f));
        }

        public static void BuildNpcs()
        {
            for (var i = 0; i < 8; i++)
            {
                var npc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                npc.transform.position = new Vector3(-8f + i * 2.2f, 1f, -8f);
                npc.name = $"NPC_{i + 1}";
                npc.AddComponent<NPC>().IsMerchant = i == 1 || i == 5;
            }
        }

        public static void BuildInteractables()
        {
            SpawnChest(new Vector3(6f, 0.6f, -4f));
            SpawnChest(new Vector3(74f, 0.6f, 72f));
            SpawnDoor(new Vector3(35f, 1.2f, 35f));
            SpawnDoor(new Vector3(35f, 1.2f, 42f));
        }

        private static void CreateArea(string name, Vector3 center, Color color)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = name;
            ground.transform.position = center;
            ground.transform.localScale = new Vector3(2.4f, 1f, 2.4f);
            ground.GetComponent<Renderer>().material.color = color;
        }

        private static void SpawnChest(Vector3 position)
        {
            var chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chest.name = "Chest";
            chest.transform.position = position;
            chest.transform.localScale = new Vector3(1f, 0.8f, 1f);
            chest.AddComponent<Chest>();
        }

        private static void SpawnDoor(Vector3 position)
        {
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.position = position;
            door.transform.localScale = new Vector3(1.4f, 2.4f, 0.2f);
            door.AddComponent<Door>();
        }
    }

    public sealed class NPC : MonoBehaviour
    {
        public bool IsMerchant;
        private void OnMouseDown()
        {
            QuestSystem.Instance.AddProgress(QuestType.Talk, 1);
            if (IsMerchant) InventorySystem.Instance.Add(InventorySystem.Instance.RandomItem());
        }
    }

    public sealed class Chest : MonoBehaviour
    {
        private bool _opened;
        private void OnMouseDown()
        {
            if (_opened) return;
            _opened = true;
            GetComponent<Renderer>().material.color = Color.yellow;
            InventorySystem.Instance.Add(InventorySystem.Instance.RandomItem());
            QuestSystem.Instance.AddProgress(QuestType.Collect, 1);
        }
    }

    public sealed class Door : MonoBehaviour
    {
        private bool _open;
        private void OnMouseDown()
        {
            _open = !_open;
            transform.rotation = _open ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
            QuestSystem.Instance.AddProgress(QuestType.Explore, 1);
        }
    }

    public sealed class DayNightWeather : MonoBehaviour
    {
        private Light _sun;
        private float _cycle;
        private void Start()
        {
            var sun = new GameObject("Sun");
            _sun = sun.AddComponent<Light>();
            _sun.type = LightType.Directional;
            RenderSettings.fog = true;
        }

        private void Update()
        {
            _cycle += Time.deltaTime * 0.02f;
            var t = Mathf.Repeat(_cycle, 1f);
            _sun.transform.rotation = Quaternion.Euler(t * 360f - 90f, 40f, 0f);
            _sun.intensity = Mathf.Lerp(0.2f, 1.2f, Mathf.Sin(t * Mathf.PI));
            RenderSettings.fogDensity = Mathf.Lerp(0.002f, 0.015f, Mathf.PerlinNoise(Time.time * 0.03f, 0f));
        }
    }
}
