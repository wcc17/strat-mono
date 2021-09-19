@echo off

cd assets
rmdir /Q /S sprites
mkdir sprites


for %%f in (./raw/*) do (
    Rem f is the path, ~nf is the name of the file
    echo %%f
    echo %%~nf

    Aseprite.exe -b raw/%%f --save-as "sprites/%%~nf_{tag}/%%~nf_{tag}.png"
)

cd ../
mono C:\Users\chris\Development\Nez\Nez.SpriteAtlasPacker\PrebuiltExecutable\SpriteAtlasPacker.exe -image:roots.png -map:roots.atlas ./assets/sprites