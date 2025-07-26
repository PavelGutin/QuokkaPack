# 🧳 QuokkaPack

**QuokkaPack** is a smart, user-friendly packing list app that helps you plan trips and organize what to bring - without forgetting anything important.

Whether you're traveling solo or prepping for a family adventure, QuokkaPack makes it easy to:

- 🧳 Create and manage trips  
- 🏷️ Organize gear into reusable categories  
- ✅ Track packing items per trip  
- 👤 Securely log in and save your personalized lists  
- 📦 Quickly reuse or customize packing templates

---

## ✨ Key Features

- **Trip planning made simple**  
  Create a new trip, choose your categories (like "Clothes", "Camera Gear", or "Hiking Essentials"), and you're ready to go.

- **Smart default suggestions**  
  Common categories are preselected to help you get started faster.

- **Reusable categories and items**  
  Build up your own set of gear and essentials - perfect for repeat travelers or hobbyists.

- **Fully cloud-connected**  
  Your data is tied to your login - access it anywhere, anytime.

- **Clean and intuitive interface**  
  Built for speed and focus, with a roadmap toward mobile-friendly design.

---

## 📍 Current Progress

- ✅ Login system using Microsoft Entra ID  
- ✅ Trip creation with category selection  
- ✅ Pre-seeded default categories and items  
- ✅ Razor Pages frontend connected to secure API  
- ✅ User-linked data model with trip/category/item support

---

## 🔜 Coming Soon

- 🧺 Packing checklist view grouped by category  
- ✏️ Edit and manage custom categories and items  
- 🔁 Duplicate trips or reuse past packing lists  
- 📱 Mobile-friendly UI  
- 📦 Docker deployment and public demo

---

## 🧱 Architecture

| Layer | Description |
|-------|-------------|
| **QuokkaPack.API** | REST API secured by Azure Entra ID |
| **QuokkaPack.RazorPages** | Frontend built with Razor Pages and secure token-based API calls |
| **QuokkaPack.Data** | EF Core DbContext and seed data for local development |
| **QuokkaPack.Shared** | Reusable models, DTOs, and mappings across projects |
| **QuokkaPack.Infrastructure** | Identity bootstrapping and user initialization |