using UnityEngine;
using Zenject;
using Cinemachine;
using System;
using System.Collections.Generic;
using TMPro;

public class SceneGameplayInstaller : MonoInstaller<SceneGameplayInstaller>
{
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;

    [SerializeField] private JSONSaveManager _jSONSaveManager;

    [SerializeField] private Game _game;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _playerParent;

    [SerializeField] private GameObject _gameplayContentFolder;
    [SerializeField] private GameObject _environmentContentFolder;

    private Map[] _mapsPool;

    public override void InstallBindings()
    {
        _mapsPool = Resources.LoadAll<Map>("Prefabs/Maps");

        Factoryes();
        Uncategorized();
        Showcase();
    }

    private void Factoryes()
    {
        Container.BindFactory<Player, Player.Factory>()
            .FromComponentInNewPrefab(_playerPrefab)
            .UnderTransform(_playerParent)
            .AsCached();

        foreach (var map in _mapsPool)
            Container.BindFactory<Map, Map.Factory>()
                .FromComponentInNewPrefab(map).AsCached();

        Container.BindFactory<GameData, GameData.Factory>()
            .AsCached();

        Container.BindFactory<Game.PlayerManager, Game.PlayerManager.Factory>()
            .AsCached();

        Container.BindFactory<Game.MapLoader, Game.MapLoader.Factory>()
            .AsCached();
    }

    private void Uncategorized()
    {
        Container.BindInstance(_virtualCamera);
        Container.QueueForInject(_virtualCamera);

        Container.BindInstance(_jSONSaveManager);
        Container.QueueForInject(_jSONSaveManager);

        Container.BindInstance(_game);
        Container.QueueForInject(_game);

        foreach (var map in _mapsPool)
            Container.Bind<Map>().FromInstance(map);

        Container.Bind<GameObject>().WithId("GameplayContentFolder").FromInstance(_gameplayContentFolder);
        Container.Bind<GameObject>().WithId("EnvironmentContentFolder").FromInstance(_environmentContentFolder);
    }

    private void Showcase()
    {
        //// BindInterfacesTo and BindInterfacesAndSelfTo
        //// 1 - самый примитивный способ забиндить каждый интерфейс, который используетс€ в типе, на этот самый тип
        //Container.Bind<IInitializable>().To<DesktopInput>().AsSingle(); // отдельный бинд интерфейса на тип
        //Container.Bind<IDisposable>().To<DesktopInput>().AsSingle(); // отдельный бинд интерфейса на тип
        //// 2 - можно забиндить все интерфейсы, которые используютс€ в типе, на этот самый тип
        //Container.BindInterfacesTo<DesktopInput>(); // бинд всех интрефейсов типа на этот тип
        //Container.Bind<DesktopInput>().AsSingle(); // бинд самого типа, чтобы интерйесы этого типа могли на него ссылатьс€
        //// 3 - можно забинди все интерфейсы типа, которые используютс€ в типе, на этот самый тип + забиндить сам тип
        //Container.BindInterfacesAndSelfTo<DesktopInput>();

        //// ѕри нескольких прив€зках должно быть  -   public Tester(List<Weapon> weapons)   - а не   -   public Tester(Weapon weapon)  
        //Container.Bind<Weapon>().To<Gun1>().FromInstance(_gun1);
        //Container.Bind<Weapon>().To<Gun2>().FromInstance(_gun2);

        //// WithId
        //Container.Bind<Weapon>().WithId("Gun111").To<Gun1>().FromInstance(_gun1);
        //Container.Bind<Weapon>().WithId("Gun222").To<Gun2>().FromInstance(_gun2);

        //// When
        //Container.Bind<Weapon>().To<Gun1>().FromInstance(_gun1).When(x => x.ObjectType == typeof(Player));
        //Container.Bind<Weapon>().To<Gun1>().FromInstance(_gun1).When(x => x.ObjectType == typeof(Projectile));

        //// WhenInjectedInto
        //Container.Bind<Weapon>().To<Gun1>().FromInstance(_gun1).WhenInjectedInto<Player>();
        //Container.Bind<Weapon>().To<Gun1>().FromInstance(_gun1).WhenInjectedInto<Projectile>();
    }
}
