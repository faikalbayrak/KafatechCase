using Interfaces;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameSceneLifetimeScope : LifetimeScope
{
    public static IObjectResolver Contanier;
    
    [SerializeField] private GameManager gameManager;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(gameManager).As<IGameManager>();
    }
}
