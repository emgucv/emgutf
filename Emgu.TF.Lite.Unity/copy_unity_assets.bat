cd Assets
cd Emgu.TF.Lite
mkdir Assets
cd Assets
mkdir Scripts
cd Scripts
rm -rf Emgu.TF.Util
mkdir Emgu.TF.Util
rm -rf Emgu.TF.Lite
mkdir Emgu.TF.Lite
rm -rf Emgu.Models
mkdir Emgu.Models
cd ..
cd ..
cd ..
cd ..

cp ../tensorflow/LICENSE Assets/Emgu.TF.Lite/Assets/Documentation/tensorflow.license.txt


cp ../Emgu.TF.Util/*.cs Assets/Emgu.TF.Lite/Assets/Scripts/Emgu.TF.Util/
cp ../Emgu.TF.Lite/*.cs Assets/Emgu.TF.Lite/Assets/Scripts/Emgu.TF.Lite/
cp ../Emgu.Models/*.cs Assets/Emgu.TF.Lite/Assets/Scripts/Emgu.Models/

