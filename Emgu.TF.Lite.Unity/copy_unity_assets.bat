cd Assets
cd Emgu.TF.Lite
rm -rf Emgu.TF.Util
mkdir Emgu.TF.Util
rm -rf Emgu.TF.Lite
mkdir Emgu.TF.Lite
rm -rf Emgu.Models
mkdir Emgu.Models
cd ..
cd ..

cp ../tensorflow/LICENSE Assets/Emgu.TF.Lite/tensorflow.license.txt
cp unityStoreIcons/README.txt Assets/Emgu.TF.Lite/README.txt

cp ../Emgu.TF.Util/*.cs Assets/Emgu.TF.Lite/Emgu.TF.Util/
cp ../Emgu.TF.Lite/*.cs Assets/Emgu.TF.Lite/Emgu.TF.Lite/
cp ../Emgu.Models/*.cs Assets/Emgu.TF.Lite/Emgu.Models/

