namespace MindMapGenerator.Core.Helper
{
    public static class GenerateMessageHelper
    {
        public static string GenerateMessage(string prompt)
        {
            return $@"
                message = {prompt} please return result only.
                Identify the type of system shown in the message and replace [SUBJECT] with system needed And return the result as shown below.
                Generate a comprehensive mind map for [SUBJECT] as React Flow JSON data, focusing on features, use cases, and practical applications. Include:

                1. Nodes array containing:
                   - Unique id 
                   - position(x,y coordinates in tree layout) 
                   - data.label (concise text describing features or use cases) 
                   - type (for layout: input, default, output) 
                   - style object with: 
                       . background (hex/rgb) 
                       . border (1px solid color) 
                       . color (text color) 
                       . width (150-220px) 
                       . borderRadius (3-8px) 

                2. Edges array with:
                   - id (unique combination) 
                   - source/target node IDs 
                   - style (stroke width: 2-3px, stroke: color) 
                   - label (dynamically generated to describe how features/use cases relate) 
                   - Optional animated (true for main connections) 

                3. Content Focus:
                   - Highlight key features of each technology
                   - Describe practical use cases
                   - Include real-world applications
                   - Show how features complement each other

                4. Style Requirements:
                   - Central node: High-contrast colors (e.g., dark blue background + white text)
                   - Feature nodes: Medium saturation colors
                   - Use case nodes: Light backgrounds with dark text
                   - Color palette should be thematically relevant to [SUBJECT]

                5. Hierarchy:
                   - Central node: Technology name
                   - Level 1: Key features
                   - Level 2: Use cases for each feature
                   - Level 3: Real-world examples or tools

                Format response as valid JSON (no markdown) using this exact structure:
                {{
                  \""nodes\"": [
                    {{
                      \""id\"": \""1\"",
                      \""type\"": \""input\"",
                      \""position\"": {{ \""x\"": 0, \""y\"": 0 }},
                      \""data\"": {{ \""label\"": \""Central Node\"" }},
                      \""style\"": {{
                        \""background\"": \""#2563eb\"",
                        \""border\"": \""2px solid #1e40af\"",
                        \""color\"": \""#ffffff\"",
                        \""width\"": 200,
                        \""borderRadius\"": 6
                      }}
                    }},
                    ...
                  ],
                  \""edges\"": [
                    {{
                      \""id\"": \""e1-2\"",
                      \""source\"": \""1\"",
                      \""target\"": \""2\"",
                      \""style\"": {{ \""stroke\"": \""#94a3b8\"", \""strokeWidth\"": 2 }},
                      \""label\"": \""supports\"",
                      \""animated\"": true
                    }},
                    ...
                  ]
                }}
                ";
        }

    }
}
