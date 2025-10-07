#!/bin/bash

if [ ! -f "$HOME/data/laz/abenberg_data_2008.zip" ]; then
  wget https://obj-web.iosb.fraunhofer.de/content/3d-sensordaten/testdaten/abenberg-als/abenberg_data_2008.zip --directory-prefix=$HOME/data/laz
fi

unzip -q -n ~/data/laz/abenberg_data_2008.zip -d ~/data/laz -x readme.txt view_data_2008.pov
rm ~/data/laz/abenberg_data_2008.zip

# download LAS tools
if [ ! -d "$HOME/Downloads/LAStools" ]; then
  sudo apt install libjpeg62 libpng-dev libtiff-dev libjpeg-dev zlib1g-dev libproj-dev liblzma-dev libjbig-dev libzstd-dev libgeotiff-dev libwebp-dev liblzma-dev libsqlite3-dev -y
  wget https://downloads.rapidlasso.de/LAStools.tar.gz --directory-prefix=~/Downloads
  tar -xzf ~/Downloads/LAStools.tar.gz --directory ~/Downloads/LAStools
fi

# convert the ASCII into a LAZ file
~/Downloads/LAStools/bin/txt2las64 -i ~/data/laz/abenberg_data_2008.txt \
                                   -set_point_type 1 \
                                   -parse 0xyz123456i \
                                   -set_scale 0.001 0.001 0.001 \
                                   -add_attribute 3 "planar shape ID" "preliminary classification" \
                                   -add_attribute 4 "normal x coord" "local normal direction estimate" 0.001 \
                                   -add_attribute 4 "normal y coord" "local normal direction estimate" 0.001 \
                                   -add_attribute 4 "normal z coord" "local normal direction estimate" 0.001 \
                                   -add_attribute 6 "sensor x coord" "sensor position" 0.0001 \
                                   -add_attribute 6 "sensor y coord" "sensor position" 0.0001 \
                                   -add_attribute 6 "sensor z coord" "sensor position" 0.0001 \
                                   -odix _temp1 -olaz
rm ~/data/laz/abenberg_data_2008.txt

# Split the file based on flight lines
~/Downloads/LAStools/bin/las2las64 -i ~/data/laz/abenberg_data_2008_temp1.laz \
                                   -subseq 0 1213495 \
                                   -odix _strip1 -olaz
~/Downloads/LAStools/bin/las2las64 -i ~/data/laz/abenberg_data_2008_temp1.laz \
                                   -subseq 1213495 1345567 \
                                   -odix _strip2 -olaz
~/Downloads/LAStools/bin/las2las64 -i ~/data/laz/abenberg_data_2008_temp1.laz \
                                   -subseq 2559062 1385526 \
                                   -odix _strip3 -olaz
~/Downloads/LAStools/bin/las2las64 -i ~/data/laz/abenberg_data_2008_temp1.laz \
                                   -subseq 3944588 1497093 \
                                   -odix _strip4 -olaz
rm ~/data/laz/abenberg_data_2008_temp1.laz

# Change the point type from 0 to 1 to have a GPS time attribute
~/Downloads/LAStools/bin/las2las64 -i ~/data/laz/abenberg_data_2008_temp1_strip*.laz \
                                   -set_point_type 1 \
                                   -odix _pt1 -olaz
rm ~/data/laz/abenberg_data_2008_temp1_strip1.laz
rm ~/data/laz/abenberg_data_2008_temp1_strip2.laz
rm ~/data/laz/abenberg_data_2008_temp1_strip3.laz
rm ~/data/laz/abenberg_data_2008_temp1_strip4.laz

# Recover the GPS time
dotnet run LasRecover.cs -- \
    -i ~/data/laz/abenberg_data_2008_temp1_strip1_pt1.laz \
    -gpstime_start 1000000 \
    -odix _rec -olaz
rm ~/data/laz/abenberg_data_2008_temp1_strip1_pt1.laz
dotnet run LasRecover.cs -- \
    -i ~/data/laz/abenberg_data_2008_temp1_strip2_pt1.laz \
    -gpstime_start 2000000 \
    -odix _rec -olaz
rm ~/data/laz/abenberg_data_2008_temp1_strip2_pt1.laz
dotnet run LasRecover.cs -- \
    -i ~/data/laz/abenberg_data_2008_temp1_strip3_pt1.laz \
    -gpstime_start 3000000 \
    -odix _rec -olaz
rm ~/data/laz/abenberg_data_2008_temp1_strip3_pt1.laz
dotnet run LasRecover.cs -- \
    -i ~/data/laz/abenberg_data_2008_temp1_strip4_pt1.laz \
    -gpstime_start 4000000 \
    -odix _rec -olaz
rm ~/data/laz/abenberg_data_2008_temp1_strip4_pt1.laz

# Merge the flight lines
~/Downloads/LAStools/bin/lasmerge64 -i ~/data/laz/abenberg_data_2008_temp1_strip1_pt1_rec.laz \
                                    -i ~/data/laz/abenberg_data_2008_temp1_strip2_pt1_rec.laz \
                                    -i ~/data/laz/abenberg_data_2008_temp1_strip3_pt1_rec.laz \
                                    -i ~/data/laz/abenberg_data_2008_temp1_strip4_pt1_rec.laz \
                                    -faf \
                                    -o ~/data/laz/abenberg_data_2008_temp2.laz
rm ~/data/laz/abenberg_data_2008_temp1_strip*_pt1_rec.laz

# Convert code 1 into ground points
~/Downloads/LAStools/bin/las2las64 -i ~/data/laz/abenberg_data_2008_temp2.laz \
                                   -keep_attribute_between 0 1 1 \
                                   -filtered_transform \
                                   -set_classification 2 \
                                   -o ~/data/laz/abenberg_data_2008_temp3.laz
rm ~/data/laz/abenberg_data_2008_temp2.laz

# Convert code 5 into vegetation
~/Downloads/LAStools/bin/las2las64 -i ~/data/laz/abenberg_data_2008_temp3.laz \
                                   -keep_attribute_between 0 5 5 \
                                   -filtered_transform \
                                   -set_classification 5 \
                                   -o ~/data/laz/abenberg_data_2008_temp4.laz
rm ~/data/laz/abenberg_data_2008_temp3.laz

# Convert code 6 into keypoints
~/Downloads/LAStools/bin/las2las64 -i ~/data/laz/abenberg_data_2008_temp4.laz \
                                   -keep_attribute_between 0 6 6 \
                                   -filtered_transform \
                                   -set_classification 8 \
                                   -o ~/data/laz/abenberg_data_2008_temp5.laz
rm ~/data/laz/abenberg_data_2008_temp4.laz

# Convert code 9 or higher into building points 
~/Downloads/LAStools/bin/las2las64 -i ~/data/laz/abenberg_data_2008_temp5.laz \
                                   -keep_attribute_above 0 8 \
                                   -filtered_transform \
                                   -set_classification 6 \
                                   -o ~/data/laz/abenberg_data_2008.laz
rm ~/data/laz/abenberg_data_2008_temp5.laz