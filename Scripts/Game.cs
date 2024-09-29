using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using TMPro;
using System;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class Game : MonoBehaviour
{
    private CompositeDisposable _disposable = new CompositeDisposable();

    protected SignalBus _signalBus;
    private JSONSaveManager _jSONSaveManager;
    private Game.PlayerManager.Factory _playerManagerFactory;
    private Game.MapLoader.Factory _mapLoaderFactory;

    private Game.MapLoader _mapLoader;
    private Game.PlayerManager _playerManager;

    private ReactiveProperty<GameState> _gameStateCurrent = new ReactiveProperty<GameState>(GameState.InBeginGame);

    public ReactiveProperty<GameState> GameStateCurrent => _gameStateCurrent;

    [Inject]
    private void Construct(
        SignalBus signalBus,
        JSONSaveManager jSONSaveManager,
        Game.PlayerManager.Factory playerManagerFactory,
        Game.MapLoader.Factory mapLoaderFactory)
    {
        _signalBus = signalBus;
        _jSONSaveManager = jSONSaveManager;
        _playerManagerFactory = playerManagerFactory;
        _mapLoaderFactory = mapLoaderFactory;
    }

    private void Awake()
    {
        _mapLoader = _mapLoaderFactory.Create();
        _playerManager = _playerManagerFactory.Create();
    }

    private void Start()
    {
        FirstGameStart();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            TryGameOver();
        if (Input.GetKeyDown(KeyCode.Escape))
            GameGoToWolrd();
        if (Input.GetKeyDown(KeyCode.R))
            GameRestart();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            GameStart(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            GameStart(2);
    }

    private void OnEnable()
    {
        SubscribeOnUniRx();
    }

    private void OnDisable()
    {
        UnSubscribeOnUniRx();
    }

    public void TryGameOver()
    {
        if (_gameStateCurrent.Value != GameState.InGame)
        {
            Debug.Log($"Игра не может быть завершена т.к. она ЕЩЕ не начата");

            return;
        }

        _signalBus.TryFire(new GameOver());

        GameRestart(3);
    }

    public void GameStart(int mapIndex)
    {
        GameBegins(mapIndex);
    }

    public void GameGoToWolrd()
    {
        GameStart(_mapLoader.MapWorldIndex);
    }

    private void GameBegins(int mapIndex)
    {
        _signalBus.TryFire(new GameBegins());

        GameStarted(mapIndex);
    }

    private void GameStarted(int mapIndex)
    {
        _mapLoader.MapLoad(mapIndex);
        _playerManager.PlayersRefresh();
    }

    private void GameRestart(float delay = 0)
    {
        CompositeDisposable disposable = new CompositeDisposable();
        Observable.Timer(TimeSpan.FromSeconds(delay))
            .Subscribe(_ =>
            {
                GameStart(_mapLoader.MapIndex);

                disposable.Clear();
            })
            .AddTo(disposable);
    }

    private void SubscribeOnUniRx()
    {
        DetermineGameState();

        _gameStateCurrent
            .Subscribe(value =>
            {
                Debug.Log($"Game State: {value}");
            })
            .AddTo(_disposable);
    }

    private void UnSubscribeOnUniRx() => _disposable.Clear();

    private void FirstGameStart()
    {
        _playerManager.PlayersCreate();
        _mapLoader.LoadMapWorld();
    }

    private void DetermineGameState()
    {
        _signalBus.GetStream<MapLoaded>()
            .Subscribe(observer =>
            {
                if (observer.Index == _mapLoader.MapWorldIndex)
                    _gameStateCurrent.Value = GameState.InWorld;
                else
                    _gameStateCurrent.Value = GameState.InGame;
            })
            .AddTo(_disposable);

        _signalBus.GetStream<GameBegins>()
            .Subscribe(observer => _gameStateCurrent.Value = GameState.InBeginGame)
            .AddTo(_disposable);

        _signalBus.GetStream<GameOver>()
            .Subscribe(observer => _gameStateCurrent.Value = GameState.InGameOver)
            .AddTo(_disposable);
    }

    public class MapLoader //: MonoBehaviour
    {
        private Game _parent;
        private List<Map.Factory> _mapFactory;
        private Map[] _mapsPool;

        private Map _map;
        private int mapIndex;

        public int MapIndex => mapIndex;
        public int MapWorldIndex => _mapsPool.Length - 1; // потому что отсчет с 0

        public MapLoader(
            Game parent,
            List<Map.Factory> mapFactory,
            Map[] maps)
        {
            _parent = parent;
            _mapFactory = mapFactory;
            _mapsPool = maps;
        }

        public void LoadMapWorld() => MapLoad(MapWorldIndex);

        public void MapLoad(int index)
        {
            if(_map)
                _map.Clear();

            Map map = _mapFactory[index].Create();
            _map = map;
            mapIndex = index;

            SetupNewMap(map, index);

            _parent._signalBus.TryFire(new MapLoaded(index));
        }

        private void SetupNewMap(Map map, int index)
        {
            map.AddSpentSurvivalTimeMax(_parent._jSONSaveManager.GameData.SpentSurvivalTimeMaxOnMaps[index]);
        }

        public class Factory : PlaceholderFactory<MapLoader> { }
    }

    public class PlayerManager
    {
        private Game _parent;

        private List<Player.Factory> _playerFactory;
        private List<Player> _players = new List<Player>();

        public PlayerManager(
            List<Player.Factory> playerFactory,
            Game parent)
        {
            _playerFactory = playerFactory;
            _parent = parent;
        }

        public void PlayersCreate()
        {
            PlayersDestroy();

            for (int i = 0; i < 1; i++)
                _players.Add(_playerFactory[0].Create());
        }

        public void PlayersRefresh()
        {
            foreach (var player in _players)
                player.Refresh();
        }

        private void PlayersDestroy()
        {
            foreach (var player in _players)
                Destroy(player.gameObject);

            _players.RemoveAll(x => x == null);
        }

        public class Factory : PlaceholderFactory<PlayerManager> { }
    }
}
