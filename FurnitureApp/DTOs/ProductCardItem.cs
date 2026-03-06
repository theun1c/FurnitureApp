namespace FurnitureApp.DTOs;

/* модель для удобной передачи данных о товарах */
public class ProductCardItem
{
    public int Id { get; set; }
    public string ProductTypeName { get; set; } =  string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Article {get; set;} = string.Empty;
    public int MinCostPartner {get; set;} 
    public string MaterialTypeName { get; set; } = string.Empty;
    public double ProductionTime { get; set; }
    
    /* поля для использования внутри view , сразу с текстом, для удобного форматирования */
    public string Title => $"{ProductTypeName} | {ProductName}";
    public string CostText => $"Минимальная стоимость для партнера: {MinCostPartner}";
    public string ArticleText => $"Артикул: {Article}";
    public string MaterialText => $"Основной материал: {MaterialTypeName}";
    public string ProductionTimeText => $"Время изготовления: {ProductionTime}";
}