# 🎮 2D Two Player Co-op Delivery and Defense

<div align="center">

[![Unity](https://img.shields.io/badge/Unity-6000.3.5f2_LTS-black?logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows_|_macOS-blue)]()
[![Status](https://img.shields.io/badge/Status-Prototype-orange)]()
[![License](https://img.shields.io/badge/License-Internal-red)]()

Prototype Turn-Based RPG built with Unity.

</div>

---

# 📖 Tentang Game

Game ini berfokus kepada implementasi Netcode for Gameobjects Unity untuk 2 Players gameplay dengan melawan threat dan mengambil supply dan diserahkan kedalam warehouse,
project ini saat ini masih berada dalam tahap **prototype development** dan digunakan untuk eksplorasi gameplay, multiplayer, authority, serta arsitektur project.

---

# ✨ Fitur

- 🎴 2 Player Gameplay
- 📊 Attack System
- 🎒 Carry and Drop System
- 🔧 Data Driven Design menggunakan ScriptableObject

---

# 🎮 Input

## Keyboard

| Tombol | Fungsi |
|---------|---------|
| W A S D / Arrow | Movement |
| Space | Attack |
| E | Carry/Drop |

---

# 📂 Struktur Folder

```text
Assets
│
├── Animation/
│   └── Player/
│
├── Prefabs/
│
├── Plugins/
│   ├── vHierarchy/
│   ├── Wingman/
│   └── Sirenix/
│
├── Scenes/
│
├── Scriptables/
|
├── Scripts/
│   └── Assemdef
|
└── Sprites/
```

---

# 🧩 Design Pattern

Pattern yang digunakan dalam project:

## Core Patterns

- Singleton (Quick Prototype)
- Observer
- State Machine

## Gameplay Patterns

- Event Driven Architecture
- Data Driven Design

## Unity Patterns

- Scriptable Object Architecture
- Component-Based Architecture
- Dependency Injection (Manual)

---

# 🔌 Plugins

| Plugin | Kegunaan |
|----------|-----------|
| Odin Inspector | Inspector Enhancement |
| Input System | Input Management |
| TextMeshPro | Text Rendering |
| Unitask | Async Programming |

---

# 📊 Coding Convention

## C#

- Menggunakan `camelCase` untuk field.
- Menggunakan `PascalCase` untuk property dan method.
- Menghindari Magic Number.
- Mengutamakan Composition dibanding Inheritance.
- Menggunakan `readonly` jika memungkinkan.

Contoh:

```csharp
private int currentHealth;

public int CurrentHealth => currentHealth;

public void TakeDamage(int amount)
{
    currentHealth -= amount;
}
```

---

# 🧪 Development Build

## Requirements

- Unity 6000.3.10f1 LTS
- Git
- Visual Studio / Rider

## Clone Repository

```bash
git clone https://github.com/AbdulAzziz2020/2D-Two-Player-Cooperative-Delivery-and-Defense-Game.git
```

## Open Project

```text
Unity Hub
↓
Add Project
↓
Select Folder
↓
Open with Unity 6000.3.5f2 LTS
```

---

# 🎮 Cara Instalasi & Bermain

## Untuk Pemain (Rilisan ZIP)

1. Unduh versi terbaru dari [Google Drive](https://drive.google.com/file/d/15LG0BeMj10-x_L0Nm2yl639lk4wEHgPJ/view?usp=sharing)
2. Ekstrak seluruh file ZIP ke satu folder.
3. Pastikan file `.exe` dan folder `_Data` berada pada lokasi yang sama.
4. Jalankan:

```text
2D Two-Player Cooperative Delivery-and-Defense Game.exe
```

5. Selamat bermain!

---

# 🐞 Known Issues

- Beberapa animasi masih menggunakan placeholder.

---

# 👨‍💻 Developer

**The Kuro Neko Team**

Built with ❤️ using Unity.

---

# 📜 License

Project ini dibuat untuk kebutuhan pengembangan internal dan pembelajaran.

All Rights Reserved.
