cd ../..
rm -rf tmp
git archive --format=tar --prefix=tmp/ HEAD | tar xf -
cd platforms
cd xamarin
cd emgutf_v1
cd component
REM c:\cygwin64\bin\mkdir.exe -p lib/ios
c:\cygwin64\bin\mkdir.exe -p lib/android
cd samples

cd android

cp -rf ../../../../../../tmp/Emgu.TF.Example/XamarinForms/XamarinForms .
cp -rf ../../../../../../tmp/Emgu.TF.Example/XamarinForms/XamarinForms.Android/* .
cp -rf ../../../../../../tmp/Emgu.TF.Models .
rm Emgu.TF.Models/CMakeLists.txt
cp -rf ../../../../../../tensorflow/tensorflow/examples/multibox_detector/data/surfers.jpg Assets/
cp -rf ../../../../../../tensorflow/tensorflow/examples/label_image/data/grace_hopper.jpg Assets/
c:\cygwin64\bin\mkdir.exe -p Resources/Raw
cp -rf ../../../../../../tensorflow/LICENSE Resources/Raw/
cp -rf ../../../../../../CommonAssemblyInfo.cs .

git checkout XamarinForms.Android.csproj

cd ..

cd ..
cd ..
cd ..


