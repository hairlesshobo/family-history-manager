import os
import json
import yaml

for entry in os.scandir("../config/yaml"):
    print(entry.path)
    name_no_ext = entry.name.split('.', 1)[0]
    json_name = f"{name_no_ext}.json"

    json_path = os.path.join("../config/profiles", json_name)

    print(json_name)

    with open(entry.path, 'r') as yaml_file:
        read_obj = yaml.safe_load(yaml_file)

        # print(read_obj)

        with open(json_path, 'w') as json_file:
            json.dump(read_obj, json_file, indent=4)

        # break

