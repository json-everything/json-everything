namespace Json.Schema.CodeGeneration.Model;

public static class CommonModels
{
	public static readonly TypeModel String = TypeModel.Simple(typeof(string).FullName);
	public static readonly TypeModel Integer = TypeModel.Simple(typeof(int).FullName);
	public static readonly TypeModel Number = TypeModel.Simple(typeof(double).FullName);
	public static readonly TypeModel Boolean = TypeModel.Simple(typeof(bool).FullName);
}