using System.Linq;
using System.Numerics;
using Content.Server._KS14.Projectiles;
using Content.Server.Movement.Components;
using Content.Shared._KS14.Projectiles;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests._KS14;

/// <summary>
/// Covers the ghost-based lag compensation ported from Klovnstation14 PR #307
/// (<see cref="LagCompProjectileSystem"/>) and the predicted-gun "hide the server duplicate"
/// mechanism it plugs into (<see cref="ShotPredictedProjectileEvent"/>).
/// </summary>
[TestFixture]
public sealed class LagCompProjectileTest
{
    private static readonly EntProtoId Mob = "CMMobMoth";

    /// <summary>
    /// Firing a physical projectile near a real, lag-compensated mob should spawn a rewound ghost
    /// for that mob and tag the projectile so it ignores the real (un-rewound) target - but only
    /// when the shooter actually has latency to compensate for.
    ///
    /// NOTE: RobustToolbox's integration test harness connects client and server over an in-process
    /// fake channel that hardcodes <c>Ping =&gt; 0</c> (see RobustIntegrationTest.NetManager.cs) - there
    /// is no way to give a test session a nonzero ping. That's also why the equivalent
    /// Content.IntegrationTests/_RMC14/RMCLagCompensationTest.cs (which relies on real ping too) has
    /// been left disabled. So this test instead covers the boundary LagCompProjectileSystem itself
    /// documents ("Ping is the ... estimate ... already relies on"): a zero-latency shot must not
    /// spawn any compensation ghosts, and must not throw doing so.
    /// </summary>
    [Test]
    public async Task ZeroPingShotSpawnsNoGhosts()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = true,
            DummyTicker = false,
        });

        var server = pair.Server;
        var sEntities = server.ResolveDependency<IEntityManager>();
        var sMapSystem = sEntities.System<SharedMapSystem>();
        var sPhysics = sEntities.System<SharedPhysicsSystem>();
        var sFixtures = sEntities.System<FixtureSystem>();

        EntityUid shooter = default;
        EntityUid mob = default;
        EntityUid projectile = default;

        await server.WaitAssertion(() =>
        {
            var player = server.PlayerMan.Sessions.First();
            Assert.That(player.AttachedEntity, Is.Not.Null);
            Assert.That(player.Ping, Is.EqualTo(0), "test harness ping assumption changed - see NOTE above");
            shooter = player.AttachedEntity!.Value;

            var shooterCoords = sEntities.GetComponent<TransformComponent>(shooter).Coordinates;

            // A real lag-compensated mob standing right next to the shooter.
            mob = sEntities.SpawnAtPosition(Mob, shooterCoords);
            sEntities.EnsureComponent<LagCompensationComponent>(mob);

            // Give it a hard circular fixture like every other mob hitbox in this game, since
            // LagCompProjectileSystem only spawns ghosts for those.
            var mobPhysics = sEntities.EnsureComponent<PhysicsComponent>(mob);
            var mobFixtures = sEntities.EnsureComponent<FixturesComponent>(mob);
            if (!mobFixtures.Fixtures.ContainsKey("fix1"))
            {
                sFixtures.TryCreateFixture(mob, new PhysShapeCircle(0.35f), "fix1", hard: true,
                    collisionLayer: (int) CollisionGroup.MobMask, collisionMask: (int) CollisionGroup.MobMask,
                    manager: mobFixtures, body: mobPhysics);
            }
            sPhysics.WakeBody(mob, body: mobPhysics);

            // A physical projectile, spawned and fired exactly like SharedGunSystem.ShootProjectile does.
            // Uses a real bullet prototype (rather than a bare entity) so it has a SpriteComponent -
            // LagCompProjectileSystem always tells the shooter's client to hide its networked
            // duplicate, and that hide is a no-op error log if there's no sprite to hide.
            projectile = sEntities.SpawnAtPosition("BulletTaser", shooterCoords);
            var projectileComp = sEntities.GetComponent<ProjectileComponent>(projectile);
            var projectilePhysics = sEntities.GetComponent<PhysicsComponent>(projectile);
            sPhysics.SetBodyStatus(projectile, projectilePhysics, BodyStatus.InAir);
            sPhysics.SetLinearVelocity(projectile, new Vector2(5f, 0f), body: projectilePhysics);

            var projectileSystem = sEntities.System<SharedProjectileSystem>();
            projectileSystem.SetShooter(projectile, projectileComp, shooter);

            var shotEv = new PlayerShotProjectileEvent(projectile, shooter);
            sEntities.EventBus.RaiseEvent(EventSource.Local, ref shotEv);
        });

        await pair.RunSeconds(0.5f);

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                // No ghost should have been spawned for a zero-latency shot - nothing to compensate for.
                // (The projectile may since have hit the real, un-rewound mob directly and been deleted -
                // that's the correct non-compensated outcome, not something this test needs to police.)
                Assert.That(sEntities.EntityQuery<LagCompensationGhostComponent>().Any(), Is.False);
            });
        });

        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// The shooter's client already has its own locally-predicted copy of a shot; the server's
    /// networked copy of that same shot should be hidden from that specific shooter via
    /// <see cref="ShotPredictedProjectileEvent"/> instead of being shown twice.
    /// </summary>
    [Test]
    public async Task ShotHidesServerDuplicateForShooter()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = true,
            DummyTicker = false,
        });

        var server = pair.Server;
        var client = pair.Client;
        var sEntities = server.ResolveDependency<IEntityManager>();
        var cEntities = client.ResolveDependency<IEntityManager>();

        EntityUid shooter = default;
        EntityUid serverProjectile = default;

        await server.WaitAssertion(() =>
        {
            var player = server.PlayerMan.Sessions.First();
            Assert.That(player.AttachedEntity, Is.Not.Null);
            shooter = player.AttachedEntity!.Value;

            var coords = sEntities.GetComponent<TransformComponent>(shooter).Coordinates;
            serverProjectile = sEntities.SpawnAtPosition("BulletTaser", coords);

            var gunSystem = sEntities.System<SharedGunSystem>();
            gunSystem.ShootProjectile(serverProjectile, new Vector2(1f, 0f), Vector2.Zero, null, shooter);
        });

        await pair.RunSeconds(0.5f);

        await client.WaitAssertion(() =>
        {
            var clientUid = pair.ToClientUid(serverProjectile);
            Assert.That(cEntities.TryGetComponent(clientUid, out SpriteComponent? sprite), Is.True);
            Assert.That(sprite!.Visible, Is.False);
        });

        await pair.CleanReturnAsync();
    }
}
