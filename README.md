# HeroDrafter ⚔️

> WPF desktop application for drafting and analyzing hero teams. Built with .NET 8, MVVM pattern, and pure ADO.NET with LocalDB.

---
<img width="1383" height="791" alt="Screenshot 2026-05-10 232910" src="https://github.com/user-attachments/assets/93cbb078-dda8-40e0-a47c-e7e3cb9eacfc" />

## 🚀 How to Run

### Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server LocalDB (comes with Visual Studio or SQL Server Express)

### Start from terminal

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/HeroDrafter.git
cd HeroDrafter

# Run the application (auto-creates DB, imports data, opens UI)
dotnet run
```

The app will:
1. Create a LocalDB database automatically on first launch
2. Import character data from the source file
3. Open the WPF window with dark gaming-style UI

---

## 🎮 Features

| Feature | Description |
|---------|-------------|
| **Hero Encyclopedia** | Browse all characters with stats, abilities, and lore |
| **Add / Edit / Delete** | Full CRUD for custom characters |
| **Draft System** | Drag heroes into 3 ally + 3 enemy slots |
| **Team Analytics** | Compare total power, balance, counter-picks, rarity bonuses |
| **Export Report** | Save draft analysis to `.txt` file |
| **Dark Gaming UI** | Styled with cyberpunk/gaming aesthetics |

---

## 🛠 Tech Stack

- **.NET 8 / WPF** — desktop framework
- **MVVM** — architecture pattern
- **ADO.NET (pure SQL)** — data access, no ORM
- **SQL Server LocalDB** — local database
- **Ukrainian UI** — full Ukrainian localization

---

## 📁 Project Structure

```
HeroDrafter/
├── Models/           # Character, Rarity, PrimaryRole
├── Data/             # DatabaseManager, DataImporter
├── Business/         # DraftAnalyzer logic
├── ViewModels/       # MVVM ViewModels
├── Views/            # MainWindow, AddEditCharacterWindow
├── Commands/         # RelayCommand
└── App.xaml(.cs)     # App entry point, DI setup
```
