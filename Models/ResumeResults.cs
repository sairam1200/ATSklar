using System.Collections.Generic;

public class ResumeResult
{
    public int ATSScore { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
}