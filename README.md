# ForTheKamera2

**Custom Camera Mod for _For The King II_**  
A BepInEx plugin that allows players to toggle cinematic camera angles during combat using the middle mouse button.

---

## 🎮 What It Does

This mod enhances your combat camera experience by allowing you to:

- Dynamically **toggle between custom camera positions** using the **middle mouse button**
- Automatically update both `Group1Turn_VCam` and `Group2Turn_VCam` (if present)
- Persist your camera positions in a simple config file (`campos.cfg`)
- Seamlessly apply camera positions in each new combat scenario
- Configure everything **without restarting the game**

---

## 💾 Installation (Manual)

### 1. Install BepInEx (if not already installed)

Download **BepInEx 5** for your platform:

👉 https://github.com/BepInEx/BepInEx/releases

1. Download the correct BepInEx release for your system (likely `BepInEx_x64_5.X.X.zip`)
2. Extract the contents into your game folder, e.g.: "C:\Program Files (x86)\Steam\steamapps\common\For The King II\"
3. Your folder should now contain:

For The King II/

-- BepInEx/

-- doorstop_config.ini

-- winhttp.dll

-- ...

4. Start the Game once to init BepInEx


### 2. Install ForTheKamera2

1. Download the latest [Release ZIP](https://github.com/pLurchi/ForTheKamera2/releases)
2. Extract the folder `ForTheKamera2` (which contains `ForTheKamera2.dll`) into: "For The King II/BepInEx/plugins/ForTheKamera2/"
3. It should now look like this: "C:\Program Files (x86)\Steam\steamapps\common\For The King II\BepInEx\plugins\ForTheKamera2\ForTheKamera2.dll"
4. Launch the game.

If everything worked, a file named `campos.cfg` will be created automatically in the same folder on first use.

## 🧩 Configuration (campos.cfg)

The `campos.cfg` file contains 3 camera positions (XYZ), one per line:

# Kamera-Positions-Config
Pos1=0,11,11
Pos2=0,8,12
Pos3=1,8,12


These are the positions you’ll cycle through during combat.  
You can **edit this file while the game is running** – the mod will re-read it each time you press the middle mouse button.

No restart needed!

---

## ⌨️ Usage

- During combat, when the main camera is `"Default_Camera"`:
  - **First press**: Saves current camera position as the origin
  - **Next presses**: Toggles between position 1 → 2 → 3 → origin → repeat
- If a new combat starts, the mod re-applies your last selected camera view when `Group1Turn_VCam` or `Group2Turn_VCam` is created

---

## 🔒 Permissions & License

This project is open-source and licensed under the **MIT License**.  
See the [`LICENSE`](LICENSE) file for full details.

---

## 👤 Author

**Mod by [@pLurchi](https://github.com/pLurchi)**  
Created using BepInEx and Harmony for personal use and the modding community.

---

## 📁 Repository

GitHub Repository:  
👉 https://github.com/pLurchi/ForTheKamera2

---

