## 1. ⚙️ Naming Conventions (C#)

All code elements must adhere strictly to the following standard C# and Unity conventions:

| Category                                | Convention                                                  | Example                                                           |
| :-------------------------------------- | :---------------------------------------------------------- | :---------------------------------------------------------------- |
| **Classes**                             | `PascalCase`                                                | `SystemManager`, `InputHandler`                                   |
| **Methods/Functions**                   | `PascalCase`                                                | `InitializeData()`, `OnWaveStart()`                               |
| **Public Fields/Properties**            | `PascalCase`                                                | `MaxSpeed`, `CooldownDuration`                                    |
| **Private Fields (Non-Serialized)**     | `camelCase` starting with underscore (`_`)                  | `_cachedValue`, `_currentState`                                   |
| **Private Fields (Serialized)**         | `camelCase` (no underscore)                                 | `playerHealth`, `dashDuration`                                    |
| **UI Labels (Non-Serialized)**          | Use the name `_logLabel`                                    | `private Label _logLabel;`                                        |
| **UI Labels (Serialized)**              | Use the name `logLabel`                                     | `[SerializeField] private Label logLabel;`                        |
| **UI Visual Elements (Non-Serialized)** | Must include `VisualElement` in the name, starting with `_` | `private VisualElement _debugPanelVisualElement;`                 |
| **UI Visual Elements (Serialized)**     | Must include `VisualElement` in the name                    | `[SerializeField] private VisualElement debugPanelVisualElement;` |
| ECS Entity                              | Use the name NameEntity and not NameAuthoring               | PlayerEntity.cs , BulletEntity.cs                                 |
| ECS Baker                               | Use nameEntityAuthoring and not just authoring              | playerEntityAuthoring , bulletEntityAuthoring                     |
* **General Naming:** Use self-explanatory names for variables, methods, and types.

---

## 2. 🏗️ Architecture & Decoupling (Industry Standard)

The following architectural patterns are **mandatory** for all systems:

### 2.1. ScriptableObject Architecture (SOAP Variables)

* **Scope:** SOAP Variables and the SOAP pattern are mandatory for all **MonoBehaviour-based systems** (systems utilizing standard Unity objects). They are **not** required, and should generally be avoided, in **Unity Entity Component System (ECS)** architecture, which uses its own decoupling methods.
* **SOAP Variables:** **CRITICAL:** Always use **SOAP Variables** (ScriptableObject variables) for any value that changes at runtime (e.g., health, score, cooldowns) in **MonoBehaviour systems**.
* **Static ScriptableObjects:** Use standard ScriptableObjects for configuration values that are shared but **do not change at runtime** (e.g., configuration data).
* **Constants:** Use **constant fields** instead of in-place constants.

### 2.2. Event-Driven Decoupling (SOAP Pattern)

* **System Decoupling:** Use the **SOAP pattern** for event-driven communication between decoupled systems.
    * The **Announcer Component** must be the component that **announces/raises the event**.
    * The **Listener Component** must be the component that **listeners for/subscribes to the event**.
    * For targeted events, the Listener must expose a unique **ScriptableEvent** instance to allow the Announcer to target specific component instances.
* **Input Handling:** All player input must use the **Industry Standard Input Manager pattern** via **SOAP Events**.

---

## 3. 🧩 Unity Component Structure & Input

### 3.1. Project File Structure (Asset Organization)
All assets must be placed in the corresponding directories:
* Code in `/Assets/Scripts`
* Materials in `/Assets/Materials`
* Prefabs in `/Assets/Prefabs`
* 3D models in `/Assets/Models`
* Scenes in `/Assets/Scenes`

### 3.2. Input System Guidelines
* The project uses the newest Input System package `com.unity.inputsystem`.
* **CRITICAL:** The AI must never edit unity-generated input system files.

### 3.3. Code Snippet Guidelines
* When providing code, only include the **changed functions** and nothing else.
* Classes and their components will always reside on the **same GameObject** (never in parent or child objects).

### 3.4. Function Placement & Visibility
* **Grid Functions:** The `CellToWorld` and `WorldToCell` functions must be declared as **`private`**.
* **Unity Events:** Never suggest or require hooking up event listeners in the Unity Inspector for `OnEnable` and `OnDisable`.
* **Comments:** Add comments for **public methods**.

---

## 4. ✍️ Whitespace and Formatting

* **Control Structures and Functions:** Always omit the space between the keyword/name and the opening parenthesis `(`.
    * **Good:** `if(condition)`, `while(condition)`, `for(int i = 0; i < 10; i++)`, `MyFunction(argument)`
    * **Bad:** `if (condition)`, `while (condition)`, `for (int i = 0; i < 10; i++)`, `MyFunction (argument)`

---

## 5. 🗣️ Communication Rules

* Be conversational but concise and professional.
* Refer to me in the second person and yourself in the first.
* Respond in the language I used to ask the question.
* Never say sorry. Instead, try your best to proceed or explain the circumstances to me without apologizing.

---

## 6. 📚 Documentation & Version Control

The following template must be used when committing this style guide file to the repository:

feature: Implement Project Style Guide Key achievements: 
- Added comprehensive style guide: MyProjectStyle.md 
- Defined strict Naming Conventions for serialised and non-serialised fields. 
- Mandated the use of SOAP architecture for MonoBehaviour systems only. 
- Confirmed whitespace rules for control structures and functions (e.g., if(condition)).

## 7. 🐙 Pull Request Template

Feature: Feature Name

📋 Description

Your text goes here

✨ Changes

**Changes Here**

🔧 Technical Details

**Files and other Dependencies info here**

✅ Testing


📦 Package Updates


🔗 Related

Branch: `Branch Name Here`

Base: `develop`

Next: `Branch Name and Tasks here`

📝 Notes

Any additional notes here