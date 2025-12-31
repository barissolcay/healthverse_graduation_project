# Git Komutları Rehberi 🚀

Bu rehber, HealthVerse projesi için sık kullanılan git komutlarını içerir.

---

## 📌 Temel Komutlar

### Değişiklikleri Kaydetme
```bash
# Tüm değişiklikleri staging'e ekle
git add -A

# Belirli bir dosyayı ekle
git add dosya_adi.cs

# Commit oluştur
git commit -m "Açıklayıcı mesaj yaz"

# Add + Commit tek satırda (sadece tracked dosyalar için)
git commit -am "Mesaj"
```

### GitHub'a Gönderme
```bash
# Değişiklikleri push et
git push

# İlk push (yeni branch için)
git push -u origin branch-adi
```

### Güncellemeleri Çekme
```bash
# Remote'dan değişiklikleri çek
git pull

# Fetch (sadece bilgi al, merge etme)
git fetch
```

---

## 🌿 Branch İşlemleri

```bash
# Mevcut branch'leri listele
git branch

# Remote branch'leri de göster
git branch -a

# Yeni branch oluştur ve geç
git checkout -b yeni-branch-adi

# Başka branch'e geç
git checkout main

# Branch sil (local)
git branch -d branch-adi

# Branch sil (force)
git branch -D branch-adi

# Remote branch sil
git push origin --delete branch-adi
```

---

## 🔍 Durum Kontrol

```bash
# Mevcut durumu gör
git status

# Kısa format
git status -s

# Commit geçmişi
git log

# Tek satır log
git log --oneline

# Son 5 commit
git log -5 --oneline

# Değişiklikleri gör (commit edilmemiş)
git diff

# Staged değişiklikleri gör
git diff --staged
```

---

## ⏪ Geri Alma İşlemleri

### Değişiklikleri İptal Etme
```bash
# Tek dosyadaki değişiklikleri geri al
git checkout -- dosya_adi.cs

# Tüm değişiklikleri geri al (DİKKAT!)
git checkout -- .

# Staging'den çıkar (değişiklik kalır)
git reset HEAD dosya_adi.cs

# Son commit'i geri al (değişiklikler kalır)
git reset --soft HEAD~1

# Son commit'i tamamen sil (DİKKAT!)
git reset --hard HEAD~1
```

### Commit Düzeltme
```bash
# Son commit mesajını değiştir
git commit --amend -m "Yeni mesaj"

# Son commit'e dosya ekle
git add unutulan_dosya.cs
git commit --amend --no-edit
```

---

## 🔀 Merge ve Rebase

```bash
# Başka branch'i mevcut branch'e merge et
git merge feature-branch

# Merge conflict çözdükten sonra
git add .
git commit -m "Merge conflict resolved"

# Merge'ü iptal et
git merge --abort

# Rebase (geçmişi düzelt)
git rebase main
```

---

## 📦 Stash (Geçici Saklama)

```bash
# Değişiklikleri geçici sakla
git stash

# Açıklama ile sakla
git stash save "WIP: Login ekranı"

# Saklananları listele
git stash list

# Son stash'i geri getir
git stash pop

# Belirli stash'i getir
git stash apply stash@{0}

# Stash sil
git stash drop stash@{0}

# Tüm stash'leri sil
git stash clear
```

---

## 🏷️ Tag İşlemleri

```bash
# Tag oluştur
git tag v1.0.0

# Açıklamalı tag
git tag -a v1.0.0 -m "İlk release"

# Tag'leri listele
git tag

# Tag'i push et
git push origin v1.0.0

# Tüm tag'leri push et
git push origin --tags
```

---

## 🛠️ Yararlı Konfigürasyonlar

```bash
# Kullanıcı bilgilerini ayarla
git config --global user.name "Barış Solcay"
git config --global user.email "email@example.com"

# Varsayılan branch adı
git config --global init.defaultBranch main

# Renkleri aç
git config --global color.ui auto

# Alias oluştur
git config --global alias.st status
git config --global alias.co checkout
git config --global alias.br branch
git config --global alias.cm "commit -m"
```

---

## 🆘 Sorun Giderme

### "Detached HEAD" Durumu
```bash
# Yeni branch oluşturarak kaydet
git checkout -b yeni-branch

# Ya da main'e geri dön
git checkout main
```

### Yanlışlıkla Push Ettiysen
```bash
# Son push'u geri al (DİKKAT: Başkaları pull ettiyse sorun olur!)
git push origin main --force
```

### Dosyayı Git'ten Çıkar (ama silme)
```bash
# .gitignore'a ekledikten sonra
git rm --cached dosya_adi.cs
git commit -m "Remove file from tracking"
```

### Remote URL Değiştirme
```bash
# Mevcut remote'u gör
git remote -v

# URL değiştir
git remote set-url origin https://github.com/yeni/repo.git
```

---

## 📋 Günlük Workflow

### Feature Geliştirirken
```bash
# 1. Main'den yeni branch
git checkout main
git pull
git checkout -b feature/yeni-ozellik

# 2. Geliştirme yap, commit et
git add -A
git commit -m "feat: Yeni özellik eklendi"

# 3. Main'i merge et (güncel kal)
git checkout main
git pull
git checkout feature/yeni-ozellik
git merge main

# 4. Push et
git push -u origin feature/yeni-ozellik

# 5. PR aç (GitHub'da)
```

### Hızlı Fix
```bash
git checkout main
git pull
git checkout -b hotfix/bug-fix
# ... düzelt ...
git add -A
git commit -m "fix: Bug düzeltildi"
git push -u origin hotfix/bug-fix
```

---

## 📝 Commit Mesajı Standartları

```
feat:     Yeni özellik
fix:      Bug düzeltme
docs:     Dokümantasyon
style:    Kod formatı (fonksiyon değişmez)
refactor: Kod iyileştirme
test:     Test ekleme
chore:    Build, config vs.
```

**Örnekler:**
- `feat: Kullanıcı login ekranı eklendi`
- `fix: Null reference hatası düzeltildi`
- `docs: README güncellendi`
- `refactor: AuthService yeniden yapılandırıldı`

---

> 💡 **İpucu:** Herhangi bir git komutunu `--help` ile çalıştırarak detaylı yardım alabilirsin:
> ```bash
> git commit --help
> git branch --help
> ```

---
*Son güncelleme: 31 Aralık 2025*
