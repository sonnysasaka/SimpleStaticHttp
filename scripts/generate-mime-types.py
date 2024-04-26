"""
Generates mime-type mapping from Apache HTTPD config.
Used by MimeTypes.cs
"""

import requests

def download_mime_types(url):
    response = requests.get(url)
    if response.status_code == 200:
        return response.text
    else:
        raise Exception(f"Failed to download file: HTTP {response.status_code}")

def parse_mime_types(mime_data):
    mime_dict = {}
    for line in mime_data.splitlines():
        if line and not line.startswith("#"):
            parts = line.strip().split()
            if len(parts) > 1:
                mime_type = parts[0]
                extensions = parts[1:]
                for ext in extensions:
                    mime_dict[ext] = mime_type
    return mime_dict

def generate_csharp_code(mime_dict):
    csharp_code = "using System.Collections.Generic;\n\n"
    csharp_code += "public static class MimeTypes\n{\n"
    csharp_code += "    public static readonly Dictionary<string, string> Mapping = new Dictionary<string, string>\n    {\n"
    for ext, mime in mime_dict.items():
        csharp_code += f'        {{"{ext}", "{mime}"}},\n'
    csharp_code = csharp_code.rstrip(',\n') + "\n    };\n}\n"
    return csharp_code

def main():
    url = "https://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types"
    mime_data = download_mime_types(url)
    mime_dict = parse_mime_types(mime_data)
    csharp_code = generate_csharp_code(mime_dict)
    print(csharp_code)

if __name__ == "__main__":
    main()
