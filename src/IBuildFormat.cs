using Engine;

public interface IBuildFormat
{
    bool Test(string path);
    
    int SaveToFile(string path, Point3 point1, Point3 point2);
    
	int PasteFromFile(string path, Point3 point3);
}