using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Koordinator utama pertandingan Arena Clash 3D (3v3 Team Deathmatch).
    /// Mengatur alur Countdown, Timer, Skor, Respawn, Seleksi Hero/Difficulty, dan Kondisi Selesai.
    /// Sesuai GDD Bagian 5.1 - 5.5, 5.7, 5.9, 5.18.
    /// </summary>
    public class ArenaMatchManager : MonoBehaviour
    {
        public static ArenaMatchManager Instance { get; private set; }

        [Header("Match Settings")]
        [SerializeField] private SO_MatchSettings _settings;
        [SerializeField] private int _targetScore = 10;
        [SerializeField] private float _matchDuration = 180f;
        [SerializeField] private float _countdownDuration = 3f;
        [SerializeField] private float _respawnTime = 4f;

        [Header("UI & Selection")]
        [SerializeField] private HeroSelectionUI _heroSelectionUI;
        [SerializeField] private ArenaHUD _hud;

        [Header("Heroes & Difficulties")]
        [SerializeField] private SO_Hero _currentHero;
        [SerializeField] private SO_Hero _bulwarkHero;
        [SerializeField] private SO_BotDifficulty _currentDifficulty;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] _blueSpawnPoints;
        [SerializeField] private Transform[] _redSpawnPoints;

        [Header("Rosters")]
        [SerializeField] private Health _playerCharacter;
        [SerializeField] private List<Health> _blueTeam = new List<Health>();
        [SerializeField] private List<Health> _redTeam = new List<Health>();

        private int _blueScore = 0;
        private int _redScore = 0;
        private float _timeRemaining = 180f;
        private MatchState _matchState = MatchState.Countdown;
        private Coroutine _countdownRoutine;
        private Coroutine _matchTimerRoutine;

        public int BlueScore => _blueScore;
        public int RedScore => _redScore;
        public float TimeRemaining => _timeRemaining;
        public MatchState CurrentMatchState => _matchState;
        public SO_Hero CurrentHero => _currentHero;
        public SO_BotDifficulty CurrentDifficulty => _currentDifficulty;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_settings != null)
            {
                _targetScore = _settings.targetEliminations;
                _matchDuration = _settings.matchDuration;
                _countdownDuration = _settings.countdownDuration;
                _respawnTime = _settings.respawnTime;
            }

            _timeRemaining = _matchDuration;
        }

        private void OnEnable()
        {
            ArenaEvents.OnCharacterDied += HandleCharacterDied;
        }

        private void OnDisable()
        {
            ArenaEvents.OnCharacterDied -= HandleCharacterDied;
        }

        private void Start()
        {
            if (_heroSelectionUI != null)
            {
                // Tampilkan panel Pilih Hero & Kesulitan sebelum match dimulai
                _heroSelectionUI.ShowPanel();
            }
            else
            {
                StartMatchCountdown();
            }
        }

        public void BindSelectionUI(HeroSelectionUI selectionUI, ArenaHUD hud)
        {
            _heroSelectionUI = selectionUI;
            _hud = hud;
        }

        public void SetHeroReferences(SO_Hero blazeHero, SO_Hero bulwarkHero, SO_BotDifficulty defaultDiff)
        {
            _currentHero = blazeHero;
            _bulwarkHero = bulwarkHero;
            _currentDifficulty = defaultDiff;
        }

        public void SetupCharacters(Health player, List<Health> blueBots, List<Health> redBots, Transform[] blueSpawns, Transform[] redSpawns)
        {
            _playerCharacter = player;
            _blueSpawnPoints = blueSpawns;
            _redSpawnPoints = redSpawns;

            _blueTeam.Clear();
            if (_playerCharacter != null) _blueTeam.Add(_playerCharacter);
            if (blueBots != null) _blueTeam.AddRange(blueBots);

            _redTeam.Clear();
            if (redBots != null) _redTeam.AddRange(redBots);
        }

        public void ApplyMatchConfiguration(SO_Hero hero, SO_BotDifficulty difficulty)
        {
            if (hero != null) _currentHero = hero;
            if (difficulty != null) _currentDifficulty = difficulty;

            // 1. Terapkan konfigurasi ke Player
            if (_playerCharacter != null && _currentHero != null)
            {
                _playerCharacter.Setup(0, _currentHero.heroName, _currentHero.maxHealth, true);

                PlayerController pc = _playerCharacter.GetComponent<PlayerController>();
                if (pc != null) pc.SetHeroData(_currentHero);

                PlayerCombat combat = _playerCharacter.GetComponent<PlayerCombat>();
                if (combat != null) combat.SetHeroData(_currentHero);

                PlayerSkillController skill = _playerCharacter.GetComponent<PlayerSkillController>();
                if (skill != null) skill.SetHeroData(_currentHero);

                Renderer rend = _playerCharacter.GetComponent<Renderer>();
                if (rend != null) rend.material.color = _currentHero.heroColor;

                if (_hud != null)
                {
                    _hud.SetHeroLabel(_currentHero.heroName);
                    if (_currentHero.portrait != null)
                    {
                        _hud.SetHeroAvatar(_currentHero.portrait);
                    }
                }
            }

            // 2. Terapkan variasi hero dan kesulitan ke Bot AI
            ConfigureBotRoster();
        }

        private void ConfigureBotRoster()
        {
            // Variasi Bot: Blue Bot 1 = Blaze, Blue Bot 2 = Bulwark
            // Red Bot 1 = Blaze, Red Bot 2 = Bulwark, Red Bot 3 = Blaze
            for (int i = 1; i < _blueTeam.Count; i++)
            {
                BotBrain brain = _blueTeam[i].GetComponent<BotBrain>();
                if (brain != null)
                {
                    SO_Hero botHero = (i % 2 == 1 && _bulwarkHero != null) ? _bulwarkHero : _currentHero;
                    brain.Setup(botHero, _currentDifficulty);
                }
            }

            for (int i = 0; i < _redTeam.Count; i++)
            {
                BotBrain brain = _redTeam[i].GetComponent<BotBrain>();
                if (brain != null)
                {
                    SO_Hero botHero = (i == 1 && _bulwarkHero != null) ? _bulwarkHero : _currentHero;
                    brain.Setup(botHero, _currentDifficulty);
                }
            }
        }

        public void StartMatchCountdown()
        {
            Time.timeScale = 1.0f;
            _blueScore = 0;
            _redScore = 0;
            _timeRemaining = _matchDuration;
            _matchState = MatchState.Countdown;

            ArenaEvents.RaiseScoreChanged(_blueScore, _redScore);
            ArenaEvents.RaiseMatchTimerUpdated(_timeRemaining);
            ArenaEvents.RaiseMatchStateChanged(MatchState.Countdown);

            ResetAllCharacterPositions();

            if (_countdownRoutine != null) StopCoroutine(_countdownRoutine);
            _countdownRoutine = StartCoroutine(CountdownRoutine());
        }

        private IEnumerator CountdownRoutine()
        {
            float timer = _countdownDuration;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            _matchState = MatchState.Playing;
            ArenaEvents.RaiseMatchStateChanged(MatchState.Playing);

            if (_matchTimerRoutine != null) StopCoroutine(_matchTimerRoutine);
            _matchTimerRoutine = StartCoroutine(MatchTimerRoutine());
        }

        private IEnumerator MatchTimerRoutine()
        {
            while (_timeRemaining > 0f && _matchState == MatchState.Playing)
            {
                _timeRemaining -= Time.deltaTime;
                ArenaEvents.RaiseMatchTimerUpdated(_timeRemaining);

                if (_timeRemaining <= 0f)
                {
                    _timeRemaining = 0f;
                    FinishMatch();
                    yield break;
                }

                yield return null;
            }
        }

        private void HandleCharacterDied(Health victim, int killerTeamId, string killerName)
        {
            if (_matchState != MatchState.Playing) return;

            // Tambah skor tim pembunuh
            if (killerTeamId == 0)
            {
                _blueScore++;
            }
            else if (killerTeamId == 1)
            {
                _redScore++;
            }

            ArenaEvents.RaiseScoreChanged(_blueScore, _redScore);

            // Cek target eliminasi 10 (AC-09)
            if (_blueScore >= _targetScore || _redScore >= _targetScore)
            {
                FinishMatch();
                return;
            }

            // Respawn karakter setelah respawnTime
            StartCoroutine(RespawnRoutine(victim));
        }

        private IEnumerator RespawnRoutine(Health victim)
        {
            if (victim == null) yield break;

            victim.gameObject.SetActive(false);
            yield return new WaitForSeconds(_respawnTime);

            if (_matchState == MatchState.Playing && victim != null)
            {
                Transform[] spawns = victim.TeamId == 0 ? _blueSpawnPoints : _redSpawnPoints;
                Vector3 spawnPos = Vector3.zero;
                if (spawns != null && spawns.Length > 0)
                {
                    spawnPos = spawns[Random.Range(0, spawns.Length)].position;
                }

                victim.transform.position = spawnPos;
                victim.ResetHealth();
            }
        }

        public void FinishMatch()
        {
            if (_matchState == MatchState.Finished) return;
            _matchState = MatchState.Finished;

            if (_matchTimerRoutine != null) StopCoroutine(_matchTimerRoutine);

            MatchResult result;
            if (_blueScore > _redScore)
            {
                result = MatchResult.BlueVictory;
            }
            else if (_redScore > _blueScore)
            {
                result = MatchResult.RedVictory;
            }
            else
            {
                result = MatchResult.Draw;
            }

            Debug.Log($"[ArenaMatchManager] Match Selesai! Hasil: {result} ({_blueScore} - {_redScore})");

            // Simpan High Score ke SaveManager jika menang
            if (SaveManager.Instance != null && _blueScore > 0)
            {
                SaveManager.Instance.SetHighScore("syafiq", _blueScore * 100);
            }

            ArenaEvents.RaiseMatchStateChanged(MatchState.Finished);
            ArenaEvents.RaiseMatchFinished(result, _blueScore, _redScore);
        }

        public void PauseMatch()
        {
            if (_matchState == MatchState.Playing)
            {
                _matchState = MatchState.Paused;
                ArenaEvents.RaiseMatchStateChanged(MatchState.Paused);
            }
        }

        public void ResumeMatch()
        {
            if (_matchState == MatchState.Paused)
            {
                _matchState = MatchState.Playing;
                ArenaEvents.RaiseMatchStateChanged(MatchState.Playing);
            }
        }

        public void RestartMatch()
        {
            Time.timeScale = 1.0f;
            StopAllCoroutines();

            if (ProjectilePool.Instance != null)
            {
                ProjectilePool.Instance.ClearPool();
            }

            // Rematch tetap memakai hero & difficulty terakhir yang dipilih
            StartMatchCountdown();
        }

        public void ResetAllCharacterPositions()
        {
            // Reset Power-Up items di arena
            foreach (var p in PowerUpItem.ActivePowerUps)
            {
                if (p != null) p.ResetPowerUp();
            }

            // Reset Player Skill & Super
            if (PlayerSkillController.Instance != null)
            {
                PlayerSkillController.Instance.ResetSkillAndSuper();
            }

            for (int i = 0; i < _blueTeam.Count; i++)
            {
                if (_blueTeam[i] == null) continue;
                Vector3 pos = (_blueSpawnPoints != null && i < _blueSpawnPoints.Length)
                    ? _blueSpawnPoints[i].position
                    : new Vector3(-10f + (i * 2f), 0f, -10f);
                _blueTeam[i].transform.position = pos;
                _blueTeam[i].ResetHealth();
            }

            for (int i = 0; i < _redTeam.Count; i++)
            {
                if (_redTeam[i] == null) continue;
                Vector3 pos = (_redSpawnPoints != null && i < _redSpawnPoints.Length)
                    ? _redSpawnPoints[i].position
                    : new Vector3(10f - (i * 2f), 0f, 10f);
                _redTeam[i].transform.position = pos;
                _redTeam[i].ResetHealth();
            }
        }
    }
}
