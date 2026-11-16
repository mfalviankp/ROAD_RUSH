# ROAD RUSH

**ROAD RUSH** adalah game 3D untuk proyek mata kuliah Komputer Grafis. Pemain mengemudi melewati jalan desa tiga jalur, menghindari kendaraan, dan mengejar rekor skor. Game dapat dimainkan secara offline di Windows.

## Gameplay

- Pindah jalur untuk menghindari mobil, truk boks, sepeda motor, becak, dan bus Harapan Jaya.
- Kecepatan dan kepadatan lalu lintas meningkat selama balapan. Setiap gelombang kendaraan menyisakan setidaknya satu jalur kosong.
- Lewati rumah, pepohonan, Koperasi Desa, Polsek, dan Dapur MBG. Suara mesin, roda, atau rantai terdengar saat kendaraan mendekat.
- Kumpulkan skor dari jarak dan kendaraan yang dilewati. Rekor disimpan secara lokal.

## Kontrol

| Tombol | Aksi |
| --- | --- |
| `A` atau `←` | Pindah satu jalur ke kiri |
| `D` atau `→` | Pindah satu jalur ke kanan |
| `Esc` atau `P` | Jeda atau lanjutkan balapan |
| `R` | Mulai ulang setelah Game Over |

## Menjalankan di Unity

Gunakan **Unity 6.6 (`6000.6.3f1`)**, Git, dan Git LFS. Proyek menggunakan Universal Render Pipeline (URP) dan Unity Input System.

```sh
git lfs install
git clone https://github.com/mfalviankp/ROAD_RUSH.git
cd ROAD_RUSH
git lfs pull
```

1. Tambahkan folder `ROAD_RUSH` ke Unity Hub dan buka dengan versi Unity di atas.
2. Setelah impor aset selesai, buka [`Assets/Scenes/Gameplay.unity`](Assets/Scenes/Gameplay.unity).
3. Tekan **Play** di Unity, lalu klik **Mulai Balap**.

## Membuat build Windows x64

1. Pastikan dukungan **Windows Standalone** tersedia pada instalasi Unity.
2. Buka menu **ROAD RUSH → Build Windows x64** di Unity.
3. Buka folder `Build-Windows-Desa` yang dihasilkan di sebelah folder proyek, lalu jalankan `ROAD_RUSH.exe`. Saat membagikan build, sertakan seluruh folder agar file data dan library Unity ikut terbawa.

Repositori ini berisi sumber proyek Unity. Build Windows dibuat melalui langkah di atas.

## Struktur proyek

| Folder | Isi |
| --- | --- |
| [`Assets/Scenes/`](Assets/Scenes/) | Scene utama dan scene bawaan Unity |
| [`Assets/Scripts/`](Assets/Scripts/) | Kontrol, lalu lintas, tabrakan, audio, dan antarmuka |
| [`Assets/RoadRush/`](Assets/RoadRush/) | Model, material, kendaraan, dan elemen jalan desa |
| [`Assets/Editor/`](Assets/Editor/) | Alat penataan scene, pengujian Play Mode, dan build Windows |

Untuk menjalankan pemeriksaan gameplay di editor, pilih **ROAD RUSH → Run Play Mode checks**. Daftar anggota tim di layar Credits dapat diisi melalui komponen **GameManager** pada scene Gameplay.
