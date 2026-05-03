# Unity Folder Forge

Unity Editor icinde hizli klasor yapisi olusturmak icin basit bir Tools paneli.

## Ozellikler

- `Tools > Unity Folder Forge` menusunden acilir.
- Istedigin kadar ana klasor ekleyebilirsin.
- Alt klasorler sadece secili ana klasorun altina eklenir.
- `Scripts` ve `Sprites` alt klasorlerini tek tusla secili klasore ekleyebilirsin.
- `Root` alanindan hedef dizini `Assets` veya `Assets/Features` gibi ayarlayabilirsin.

## Kurulum

### Assets klasorune kopyalama

`UnityFolderForge` klasorunu Unity projenin `Assets` klasoru altina koy.

```text
Assets/UnityFolderForge
```

### Unity Package Manager ile ekleme

GitHub repo URL'sini Unity Package Manager'da `Add package from git URL` secenegiyle ekleyebilirsin.

## Kullanim

1. Unity menuden `Tools > Unity Folder Forge` sec.
2. `Add Folder` ile ana klasor ekle.
3. Alt klasor eklemek istedigin ana klasor satirinda `Select` sec.
4. `Add Subfolder` ile sadece secili klasore alt klasor ekle.
5. Istersen `Use Scripts + Sprites` ile secili klasore hizlica `Scripts` ve `Sprites` ekle.
6. `Create Folders` butonuna bas.

## Ornek

`Player` seciliyken `Scripts` ve `Sprites` eklenirse:

```text
Assets/Player
Assets/Player/Scripts
Assets/Player/Sprites
```

`Enemy` seciliyken sadece `Prefabs` eklenirse:

```text
Assets/Enemy
Assets/Enemy/Prefabs
```

`Player` icin eklenen alt klasorler `Enemy` altina otomatik acilmaz.
