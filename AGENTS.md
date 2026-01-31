# InfinityMachine - Agent Coding Guidelines

## Quick Reference

| Task | Command |
|------|---------|
| Build | `& $UNITY -batchmode -quit -projectPath . -buildTarget StandaloneWindows64` |
| All tests | `& $UNITY -batchmode -quit -projectPath . -runTests -testPlatform playmode` |
| Single test | `& $UNITY ... -testFilter "ClassName.TestName"` |
| Format | `dotnet format infinityMachine.sln` |

## Build & Test Commands

```powershell
# Run ALL PlayMode tests
& $UNITY -batchmode -quit -projectPath . -runTests -testPlatform playmode -testResults TestResults.xml

# Run single test by name
& $UNITY -batchmode -quit -projectPath . -runTests -testPlatform playmode -testFilter "PlayerMovementTests.TestJump" -testResults TestResults.xml

# Run EditMode tests (unit tests - faster, no scene)
& $UNITY -batchmode -quit -projectPath . -runTests -testPlatform editmode -testResults TestResults.xml

# Build headless
& $UNITY -batchmode -quit -projectPath . -buildTarget StandaloneWindows64
```

## Code Style

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase noun | `PlayerController` |
| Methods | PascalCase verb | `TakeDamage()` |
| Public fields | camelCase | `moveSpeed` |
| Private fields | `_camelCase` | `_controller` |
| Constants | UPPER_SNAKE | `MAX_HEALTH` |
| Interfaces | `I` prefix | `IDamageable` |
| ScriptableObjects | `SO` suffix | `WeaponDataSO` |
| Events | `On` prefix | `OnPlayerDeath` |

### Formatting

- 4 spaces indentation (no tabs)
- Allman braces (opening brace on new line)
- Max 120 chars per line
- One blank line between methods

### Imports (in order)

```csharp
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
```

### Unity Attributes (in order)

```csharp
[Header("Movement")]
[Tooltip("Units per second")]
[SerializeField, Range(0f, 20f)] private float _speed = 5f;
```

## Unity Patterns

### Unity Lifecycle Order

```csharp
Awake()      // Initialize self
OnEnable()   // Subscribe to events
Start()      // Initialize with dependencies
Update() / FixedUpdate() / LateUpdate()
OnDisable()  // Unsubscribe from events
OnDestroy()  // Cleanup
```

### Component Access

```csharp
// GOOD: TryGetComponent
if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
{
    damageable.TakeDamage(_damage);
}

// AVOID: GetComponent + null check
var component = GetComponent<Foo>();
if (component != null) { ... }
```

### Singleton Pattern

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

### Coroutine Lifecycle

```csharp
private Coroutine _routine;

private void StartRoutine()
{
    if (_routine != null) StopCoroutine(_routine);
    _routine = StartCoroutine(MyCoroutine());
}
```

### CharacterController Teleport

```csharp
// Must disable CC before moving transform
CharacterController cc = player.GetComponent<CharacterController>();
cc.enabled = false;
player.transform.position = targetPosition;
cc.enabled = true;
```

## Error Handling

```csharp
// Early exit
if (target == null) return;

// Contextual logging
Debug.LogWarning($"[{nameof(PlayerController)}] Missing: {nameof(_weapon)}");

// Dev-time assertions (stripped in builds)
Debug.Assert(_rigidbody != null, "Rigidbody required", this);
```

## Project Structure

```
Assets/_Game/
├── Scripts/
│   ├── Player/         # PlayerController, etc.
│   ├── Systems/        # RoomTeleporter, GameManager
│   ├── Enemies/        # Enemy AI
│   └── UI/             # UI controllers
├── Prefabs/
├── Scenes/
├── Art/
│   ├── Materials/
│   └── Models/
└── Settings/
```

## Testing

```csharp
using NUnit.Framework;
using UnityEngine.TestTools;

[TestFixture]
public class PlayerTests
{
    [Test]
    public void TakeDamage_ReducesHealth()
    {
        var health = new Health(100);
        health.TakeDamage(25);
        Assert.AreEqual(75, health.Current);
    }
    
    [UnityTest]
    public IEnumerator Regen_RestoresOverTime()
    {
        var player = new GameObject().AddComponent<PlayerHealth>();
        player.TakeDamage(50);
        yield return new WaitForSeconds(1f);
        Assert.Greater(player.Current, 50);
    }
}
```

## Do NOT

- Use `as any` equivalents (unsafe casts without checks)
- Suppress warnings with `#pragma warning disable`
- Leave empty catch blocks
- Delete failing tests to "pass"
- Use `GetComponent` in Update() (cache in Awake/Start)
