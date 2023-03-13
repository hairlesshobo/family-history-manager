#!/usr/bin/env bash
##############################################################
# Family History Manager - https://code.foxhollow.cc/fhm/
# 
# A cross platform tool to help organize and preserve all types
# of family history
# 
# Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
#
# This Source Code Form is subject to the terms of the Mozilla Public
# License, v. 2.0. If a copy of the MPL was not distributed with this
# file, You can obtain one at https://mozilla.org/MPL/2.0/.
#
##############################################################

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

size="48"
color="#FFFFFF"
input_directory="coreui-icons-free"
output_directory="${SCRIPT_DIR}/../source/FoxHollow.FHM/Assets/${input_directory}/${size}"
output_file_prepend="light_"
output_file_append="_${size}"

mkdir -p $output_directory

for i in $(ls -1 ${SCRIPT_DIR}/${input_directory}); do
    file_basename="$(echo $i | cut -d. -f1)"
    input="${SCRIPT_DIR}/${input_directory}/$i"
    output="${output_directory}/${output_file_prepend}${file_basename}${output_file_append}.png"

    echo "Processing $i..."
    sed -e "s/var(--ci-primary-color, currentColor)/$color/" $input | inkscape -w $size -h $size -p -o $output
done
