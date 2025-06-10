using System;
using System.Collections.Generic;

[Serializable]
public class NotesInfo
{
    public List<float> noteBeat = new();
    public List<float> noteFadeTime = new();
    public List<SirVector> notePos = new();
    public List<string> noteType = new();    
}
