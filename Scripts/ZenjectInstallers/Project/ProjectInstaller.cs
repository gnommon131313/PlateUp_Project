using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller<ProjectInstaller>
{
    public override void InstallBindings()
    {
        BindInput();
        Signals();
    }

    private void BindInput()
    {
        Container.BindInterfacesAndSelfTo<InputHandler>().AsSingle();
    }

    private void Signals()
    {
        SignalBusInstaller.Install(Container);

        Container.DeclareSignal<OrderExpired>();
        Container.DeclareSignal<MapLoaded>();
        Container.DeclareSignal<GameOver>();
        Container.DeclareSignal<GameBegins>();
        Container.DeclareSignal<MapSwitcherOpen>();
        Container.DeclareSignal<MapSwitcherClose>();
    }
}
