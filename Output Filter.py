startBlacklist = ["[d_messaging_ap]", "Thread started: ","[ForceDarkHelper]", "[monodroid-assembly]", "[chatty]", "[Looper]", "[AdrenoGLES]", "[DynamiteModule]", "[ProviderInstaller]", "[FeatureParser]",
                  "Resolved pending breakpoint", "[Perf]", "[Activity]", "[TabLayout]", "[NativeCrypto]", "[NetworkSecurityConfig]", "[AdrenoUtils]", "[RenderThread]", "[Gralloc3]", "[LB]", "[OpenGLRenderer]", "[Choreographer]",
                  "[Toast]", "[ViewRootImpl]", "[AssistStructure]", ]




def filterLine(outputLine):
    colourCode = ""
    if(outputLine.startswith(tuple(startBlacklist))):
        return
    if(outputLine.startswith("Log: ")):
        outputLine = '\'' + outputLine
    if(outputLine.lstrip().startswith("Error: ")):
        outputLine = "#"+outputLine
    
    print(outputLine)

outputLine = input("")
lines = []
while len(outputLine)>0:
    lines.append(outputLine)
    outputLine = input("")
            
for line in lines:
    filterLine(line)
