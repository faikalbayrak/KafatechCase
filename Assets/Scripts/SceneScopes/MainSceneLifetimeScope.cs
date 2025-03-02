using Client;
using Interfaces;
using Managers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainSceneLifetimeScope : LifetimeScope
{
    [SerializeField] private ClientManager clientManager;
    [SerializeField] private AudioManager audioManager;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(clientManager).As<IClientManager>();
        builder.RegisterInstance(audioManager).As<IAudioService>();
    }
}
