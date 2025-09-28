namespace PurestAdmin.SqlSugar.Entity;

/// <summary>
/// Demo Age Entity
/// </summary>
[SugarTable("DEMOAGE")]
public partial class DemoAgeEntity : BaseEntity
{
    /// <summary>
    /// Age
    /// </summary>
    [SugarColumn(ColumnName = "Age")]
    public int? Age { get; set; }
    
    /// <summary>
    /// Birthday
    /// </summary>
    [SugarColumn(ColumnName = "Birthday")]
    public DateTime? Birthday { get; set; }
    
    /// <summary>
    /// Name
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }
    
    // Xóa property Id vì đã có trong BaseEntity
}