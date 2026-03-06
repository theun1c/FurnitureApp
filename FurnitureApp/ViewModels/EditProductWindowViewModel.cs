using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FurnitureApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FurnitureApp.ViewModels;

public class EditProductWindowViewModel : ViewModelBase
{
    public int ProductId { get; }

    public ObservableCollection<ProductType> ProductTypes { get; } = new();
    public ObservableCollection<MaterialType> MaterialTypes { get; } = new();
    
    private string _productName = string.Empty;

    public string ProductName
    {
        get => _productName;
        set
        {
            _productName = value;
            OnPropertyChanged();
        }
    }
    
    private string _article = string.Empty;

    public string Article
    {
        get => _article;
        set
        {
            _article = value;
            OnPropertyChanged();
        }
    }
    
    private string _minCostPartnerText = string.Empty;
    public string MinCostPartnerText
    {
        get => _minCostPartnerText;
        set
        {
            _minCostPartnerText = value;
            OnPropertyChanged();
        }
    }
    
    private ProductType? _selectedProductType;
    public ProductType? SelectedProductType
    {
        get => _selectedProductType;
        set
        {
            _selectedProductType = value;
            OnPropertyChanged();
        }
    }
    
    private MaterialType? _selectedMaterialType;
    public MaterialType? SelectedMaterialType
    {
        get => _selectedMaterialType;
        set
        {
            _selectedMaterialType = value;
            OnPropertyChanged();
        }
    }
    
    private string _errorMessage =  string.Empty;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }
    
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public EditProductWindowViewModel(int productId)
    {
        ProductId = productId;
    }

    public async Task LoadDataAsync()
    {
        var options = new DbContextOptionsBuilder<_41pProductsContext>()
            .UseNpgsql("Host=edu.ngknn.ru;Port=5442;Database=41P_products;Username=21P;Password=123;")
            .Options;

        await using var context = new _41pProductsContext(options);

        var productTypes = await context.ProductTypes
            .OrderBy(x => x.ProductType1)
            .ToListAsync();

        var materialTypes = await context.MaterialTypes
            .OrderBy(x => x.MaterialType1)
            .ToListAsync();

        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == ProductId);

        if (product is null)
        {
            ErrorMessage = "Товар для редактирования не найден.";
            return;
        }

        ProductTypes.Clear();
        foreach (var type in productTypes)
        {
            ProductTypes.Add(type);
        }

        MaterialTypes.Clear();
        foreach (var material in materialTypes)
        {
            MaterialTypes.Add(material);
        }

        ProductName = product.Name;
        Article = product.Article;
        MinCostPartnerText = product.MinCostPartner.ToString();

        SelectedProductType = ProductTypes.FirstOrDefault(x => x.Id == product.IdProductType);
        SelectedMaterialType = MaterialTypes.FirstOrDefault(x => x.Id == product.IdMaterialType);
    }

    public async Task<bool> SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ProductName))
        {
            ErrorMessage = "Введите наименование товара";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Article))
        {
            ErrorMessage = "Введите артикул";
            return false;
        }

        if (SelectedProductType is null)
        {
            ErrorMessage = "Выберите тип продукта";
            return false;
        }

        if (SelectedMaterialType is null)
        {
            ErrorMessage = "Выберите материал";
            return false;
        }

        if (!int.TryParse(MinCostPartnerText, out var minCostPartner))
        {
            ErrorMessage = "Стоимость должна быть числом";
            return false;
        }

        var options = new DbContextOptionsBuilder<_41pProductsContext>()
            .UseNpgsql("Host=edu.ngknn.ru;Port=5442;Database=41P_products;Username=21P;Password=123;")
            .Options;

        await using var context = new _41pProductsContext(options);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == ProductId);

        if (product is null)
        {
            ErrorMessage = "Товар для сохранения не найден.";
            return false;
        }

        product.Name = ProductName;
        product.Article = Article;
        product.MinCostPartner = minCostPartner;
        product.IdProductType = SelectedProductType.Id;
        product.IdMaterialType = SelectedMaterialType.Id;

        await context.SaveChangesAsync();
        return true;
    }
}