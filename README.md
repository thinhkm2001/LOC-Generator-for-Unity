# 📊 Unity LOC Generator Tool

A lightweight yet extensible **Unity Editor tool** for analyzing your codebase using **Lines of Code (LOC)** metrics.
It helps you quickly identify large scripts, monitor code growth, and support refactoring decisions.

<p align="center">
  <img width="500px" src="/Screenshot%202026-04-18%20225417.png?raw=true" alt="Demo">
</p>

---

## 🚀 Features

* 🔍 Scan `.cs` files in your project
* 📁 Configurable:

  * Include folders
  * Ignore folders
* 📊 In-editor visualization:

  * File-level LOC
  * Total LOC
  * Largest file detection
* 🔽 Sorting:

  * By line count (asc/desc)
  * By file path
* 📌 Ping file in Project window
* 💾 Export report to `.txt`
* 📥 Import report from `.txt`
* 🔁 Reusable data (no re-scan needed for sorting/filtering)

---

## 🧠 Why this tool?

When working on medium-to-large Unity projects, it becomes difficult to:

* Identify **overly large scripts**
* Track **codebase growth**
* Decide **where to refactor**

This tool provides a **quick structural overview** of your codebase using LOC as a simple but effective metric.

---

## 📦 Installation

1. Copy file into your project:

```
Assets/Editor/LOCGeneratorWindow.cs
```

2. Open Unity

3. Access tool via menu:

```
Tools → LOC Generator
```

---

## 🧭 Usage

### 1. Configure folders

* **Include Folders**

  * Where to scan (e.g. `Assets/Scripts`)
* **Ignore Folders**

  * Exclude paths (e.g. `Assets/Thirdparties`, `Assets/Plugins`)

---

### 2. Generate LOC

Click:

```
Generate
```

This will:

* Scan all `.cs` files
* Calculate LOC per file
* Display results in the window

---

### 3. Sort results

Use dropdown:

```
Sort By:
- LinesDesc
- LinesAsc
- Path
```

---

### 4. Export report

Click:

```
Export File
```

Output example:

```
Assets/Scripts/MyScript.cs - 120 lines
Assets/Scripts/Player.cs - 340 lines
...
Total lines of code: 7842
```

---

### 5. Import report

Click:

```
Import File
```

* Load previously exported `.txt`
* Rebuild full dataset inside tool
* Useful for:

  * Sharing between team members
  * Comparing snapshots

---

## 📄 Report Format

The tool expects this format:

```
<file_path> - <line_count> lines
...
Total lines of code: <total>
```

⚠️ Do not change format unless you also update the importer.

---

## 🏗️ Architecture

The tool is structured for extensibility:

### 1. Generator

Scans project and builds data

```
LOCGenerator.Generate(...)
```

### 2. Importer

Parses existing report file

```
LOCImporter.ImportFromFile(...)
```

### 3. Exporter

Writes report to file

```
LOCExporter.ExportToFile(...)
```

### 4. Editor Window

Handles:

* UI
* Sorting
* User interaction

---

## 📊 Practical Use Cases

* 🔎 Detect **God Classes** (>300–500 LOC)
* 🧱 Identify refactor targets
* 📈 Track project growth over time
* 🧪 Compare code size between branches (manual)
* 👨‍💻 Support code reviews

---

## ⚠️ Limitations

* LOC ≠ code quality
* Does not measure:

  * Complexity (Cyclomatic)
  * Maintainability index
* Counts **physical lines**, not logical statements

---

## 🔧 Recommended Improvements

If you plan to extend this tool:

### 🔥 High value

* LOC Diff (compare 2 reports)
* Highlight large files (color UI)
* Group by folder
* CSV / JSON export

### ⚡ Advanced

* Incremental scan (file hash / timestamp)
* Code complexity metrics
* Integration with CI pipeline

---

## 💡 Example Insight

From a report like this:

* 700+ LOC file → candidate for splitting
* Folder with 60% LOC → architectural hotspot
* Sudden LOC increase → possible feature spike or tech debt

---

## 🤝 Contributing

You can extend this tool by:

* Adding new exporters
* Improving parsing logic
* Enhancing UI/UX
* Adding analytics features

---

## 📜 License

Use freely for personal or internal team projects.

---

## 🏷️ Keywords

Unity, Editor Tool, LOC, Code Metrics, Refactoring, Code Analysis
