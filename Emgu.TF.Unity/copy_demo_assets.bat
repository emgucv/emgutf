rm -rf Assets/Emgu.TF/Emgu.TF.Demo
REM mkdir Assets\Emgu.TF\Emgu.TF.Demo
mkdir -p Assets\Emgu.TF\Emgu.TF.Demo\Resources

cp ../tensorflow/tensorflow/examples/multibox_detector/data/surfers.jpg Assets/Emgu.TF/Emgu.TF.Demo/Resources

cp -rf demo/* Assets/Emgu.TF/Emgu.TF.Demo
cp unityStoreIcons/EmguTFLogo_128x128.png Assets/Emgu.TF/Emgu.TF.Demo/EmguTFLogo.png