# InfinityMachine - Agent Coding Guidelines

## Build & Test Commands

### Unity Editor Operations (via Unity CLI)
```powershell
# Windows - Get Unity path (adjust version as needed)
$UNITY = "C:\Program Files\Unity\Hub\Editor\6000.0.0f1\Editor\Unity.exe"

# Build project (headless)
& $UNITY -batchmode -quit -projectPath . -buildTarget StandaloneWindows64

# Run ALL PlayMode tests
& $UNITY -batchmode -quit -projectPath . -runTests -testPlatform playmode -testResults TestResults.xml

# Run single test by name
& $UNITY -batchmode -quit -projectPath . -runTests -testPlatform playmode -testFilter "PlayerMovementTests.TestJump" -testResults TestResults.xml

# Run test class/namespace
& $UNITY -batchmode -quit -projectPath . -runTests -testPlatform playmode -testFilter "PlayerMovementTests" -testResults TestResults.xml

# Run EditMode tests (unit tests)
& $UNITY -batchmode -quit -projectPath . -runTests -testPlatform editmode -testResults TestResults.xml

# Code coverage report
& $UNITY -batchmode -quit -projectPath . -runTests -testPlatform playmode -coverage -coverageResultsPath Coverage/ -coverageOptions "generateHtmlReport;generateBadgeReport"
```

### IDE & CLI Tools
```powershell
# Format code (from solution root)
dotnet format infinityMachine.sln --severity info

# Run tests via dotnet (if using .NET test adapter)
dotnet test --filter "FullyQualifiedName~TestClassName"
```

## Code Style Guidelines

### General Principles
- Write clean, self-documenting code with clear intent
- Follow SOLID principles; prefer composition over inheritance
- Keep methods short (under 50 lines); single responsibility
- Avoid magic numbers; use named constants or `[SerializeField]` ranges
- Use `var` for locals when type is obvious from right-hand side

### Naming Conventions
| Element | Convention | Example |
|---------|-----------|---------|
| Classes | PascalCase, noun | `PlayerController` |
| Methods | PascalCase, verb | `CalculateDamage()` |
| Variables | camelCase | `playerHealth` |
| Constants | UPPER_SNAKE_CASE | `MAX_HEALTH = 100` |
| Private fields | `_camelCase` | `_instance` |
| Properties | PascalCase | `public int Health { get; set; }` |
| Interfaces | `I` prefix | `IDamageable` |
| ScriptableObjects | Suffix `SO` | `WeaponDataSO` |
| Events | Prefix `On` | `OnPlayerDeath` |

### Unity Attributes (use in this order)
```csharp
[Header("Movement")]
[Tooltip("Units per second")]
[SerializeField, Range(0f, 20f)] private float _speed = 5f;

[Space(10)]
[Header("Combat")]
[SerializeField] private int _maxHealth = 100;

[HideInInspector] public bool isInitialized;
```

### Unity-Specific Patterns
```csharp
// Singleton pattern (lazy, thread-safe)
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

// Safe component access
private void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
    {
        damageable.TakeDamage(_damage);
    }
}

// Coroutine lifecycle
private Coroutine _moveRoutine;

private void StartMoving()
{
    StopMoving();
    _moveRoutine = StartCoroutine(MoveCoroutine());
}

private void StopMoving()
{
    if (_moveRoutine != null)
    {
        StopCoroutine(_moveRoutine);
        _moveRoutine = null;
    }
}
```

### Unity Event Method Order
Place Unity callbacks in lifecycle order:
1. `Awake()` - Initialize self
2. `OnEnable()` - Subscribe to events
3. `Start()` - Initialize with dependencies
4. `Update()` / `FixedUpdate()` / `LateUpdate()`
5. `OnDisable()` - Unsubscribe from events
6. `OnDestroy()` - Cleanup

### Error Handling
```csharp
// Early exit pattern
if (target == null) return;

// TryGetComponent over GetComponent + null check
if (hit.collider.TryGetComponent<Enemy>(out var enemy))
{
    enemy.TakeDamage(amount);
}

// Contextual logging
Debug.LogWarning($"[{nameof(PlayerController)}] Missing reference: {nameof(_weapon)}");

// Assertions for dev-time checks (stripped in builds)
Debug.Assert(_rigidbody != null, "Rigidbody required", this);
```

### Imports (order matters)
```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// Project namespaces last
using InfinityMachine.Core;
```

### Formatting
- 4 spaces indentation (no tabs)
- Allman braces (opening brace on new line)
- Max 120 characters per line
- One blank line between methods
- No trailing whitespace

## Project Structure
```
Assets/
├── Scripts/
│   ├── Runtime/
│   │   ├── Core/           # Managers, services, singletons
│   │   ├── Entities/       # Player, enemies, interactables
│   │   ├── Systems/        # Combat, audio, input systems
│   │   ├── UI/             # UI controllers, views
│   │   └── Data/           # ScriptableObjects, configs
│   ├── Editor/             # Custom inspectors, tools
│   └── Tests/
│       ├── EditMode/       # Unit tests (fast, no scene)
│       └── PlayMode/       # Integration tests (scene required)
├── Scenes/
├── Prefabs/
├── ScriptableObjects/
└── Resources/              # Runtime-loaded assets only
```

### Assembly Definitions
Create `.asmdef` files to:
- Reduce compile times (only recompile changed assemblies)
- Enforce architecture boundaries
- Enable unit testing of runtime code

```
Scripts/Runtime/Core/InfinityMachine.Core.asmdef
Scripts/Runtime/Entities/InfinityMachine.Entities.asmdef
Scripts/Editor/InfinityMachine.Editor.asmdef
Scripts/Tests/EditMode/InfinityMachine.Tests.EditMode.asmdef
```

## Testing Patterns
```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PlayerHealthTests
{
    [Test]
    public void TakeDamage_ReducesHealth()
    {
        // Arrange
        var health = new Health(100);
        
        // Act
        health.TakeDamage(25);
        
        // Assert
        Assert.AreEqual(75, health.Current);
    }
    
    [UnityTest]
    public IEnumerator Regeneration_RestoresHealthOverTime()
    {
        var player = new GameObject().AddComponent<PlayerHealth>();
        player.TakeDamage(50);
        
        yield return new WaitForSeconds(1f);
        
        Assert.Greater(player.Current, 50);
    }
}
```

## IDE Setup
- VS Code with C# Dev Kit + Unity extension
- Enable Format on Save
- OmniSharp: Enable Import Completion = true
- Files: Insert Final Newline = true
