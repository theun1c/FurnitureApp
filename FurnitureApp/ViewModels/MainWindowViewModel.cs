using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FurnitureApp.DTOs;
using FurnitureApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FurnitureApp.ViewModels;

/* именно эта вью модел является дата контекстом main window , а значит данные берутся от суда */
public partial class MainWindowViewModel : ViewModelBase
{
    
    /* коллекция карточек в главном вью модел. это типо ДТО.
     обсервабл коллекшн, потому что она уведомляет интерфейс и изменения автоматом появляются*/
    /*обсервабл коллекшн видит каждое изменение продукт кард айтем, поэтому нигде нет возвратов,
     стоит просто подключить это свойство к вью и вью будет обновляться вместо с свойством в ИРЛ*/
    public ObservableCollection<ProductCardItem> ProductCardItems { get; } = new();

    public bool IsEmpty => ProductCardItems.Count == 0;
    
    /* этот обсервабл коллекшн нужен для комбо бокса списков. важно чтобы список работал с ЮАЙ */
    public ObservableCollection<string> ProductTypes { get; } = new();

    /* для сортировки по возрастанию убыванию цены */
    public ObservableCollection<string> SortOption { get; } = new();
    
    /*нужно для всего списка, чтобы восстанавливаться после поиска.
     Лист, потому что не привязан к ЮАЙ, а просто хранит данные*/
    private List<ProductCardItem> _allProducts = new();

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                UpdateProductList();
            }
        }
    }
    
    private string _selectedProductType = string.Empty;

    public string SelectedProductType
    {
        get => _selectedProductType;
        set
        {
            /*когда выбираем новое значение из комбо бокса вызываем метод обновления списка*/
            if (_selectedProductType != value)
            {
                _selectedProductType = value;
                OnPropertyChanged();
                UpdateProductList();
            }
        }
    }
    
    private string _selectedSortOption = string.Empty;

    public string SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (_selectedSortOption != value)
            {
                _selectedSortOption = value;
                OnPropertyChanged();
                UpdateProductList();
            }
        }
    }

    /*/* после изменения  Text="{Binding SearchText}"/> вызывается поиск#1#
    private void ApplySearch()
    {
        /* сначала показываем все товары#1#
        var filteredProducts = _allProducts;

        /* если строка пустая, то поиск не применяем #1#
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            /* выбираем те товары, у которых название содержит строку поиска#1#
            filteredProducts = _allProducts
                .Where(p => p.ProductName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        
        ProductCardItems.Clear();

        /* заполняем текущий список
         тут он обновляется и уведомляет обсерваблколлектион#1#
        foreach (var product in filteredProducts)
        {
            ProductCardItems.Add(product);
        }
    }*/

    private void UpdateProductList()
    {
        IEnumerable<ProductCardItem> filteredProducts = _allProducts;

        /*сначала идет поиск */
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filteredProducts = filteredProducts
                .Where(p => p.ProductName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        /*потом фильтр по типу. это нужно чтобы все условия работали вместе*/
        if (!string.IsNullOrWhiteSpace(SelectedProductType) && SelectedProductType != "Все типы")
        {
            filteredProducts = filteredProducts
                .Where(p => p.ProductTypeName == SelectedProductType);
        }

        /* применяем сортировку. если совпадений в свиче нет - возвращаем список без сортировки, иначе сортируем*/
        filteredProducts = SelectedSortOption switch
        {
            "Стоимость по возрастанию" => filteredProducts.OrderBy(p => p.MinCostPartner),
            "Стоимость по убыванию" => filteredProducts.OrderByDescending(p => p.MinCostPartner),
            _ => filteredProducts
        };
        
        ProductCardItems.Clear();
        
        foreach (var product in filteredProducts)
        {
            ProductCardItems.Add(product);
        }
        
        OnPropertyChanged(nameof(IsEmpty));
        
    }
    
    /* конструктор с вызовом метода загрузки карточек */
    public MainWindowViewModel()
    {
        /* когда создается вью модел, сразу загружаются карточки */
        /*Логика такая:
           1.App.axaml.cs создаёт MainWindowViewModel
           2.Конструктор MainWindowViewModel вызывается
           3.Внутри конструктора вызывается LoadProducts()
           4.Коллекция ProductCardItems наполняется
           5.MainWindow.axaml показывает данные через ItemsControl*/
        _ = LoadProductsAsync(); 
    }
    
    /* метод загрузки карточек */
    private async Task LoadProductsAsync()
    {
        try
        {
            /* создаем опшнс для подключения к базе данных */
            /*опшнс - набор настроек для базы данных. тут ef core узнает к какой бд подключаться, какой провайдер использовать*/
            var options = new DbContextOptionsBuilder<_41pProductsContext>()
                .UseNpgsql("Host=edu.ngknn.ru;Port=5442;Database=41P_products;Username=21P;Password=123")
                .Options;

            /* создаем контекст */
            /*главный объект ef для работы с БД. через него мы обращаемся к таблицам
             он умеет создавать, добавлять, изменять, удалять данные*/
            /*Почему using var:
               Потому что контекст использует ресурсы подключения к БД.
               Когда метод завершится, context будет автоматически освобождён.
               То есть:
               •создали контекст
               •прочитали данные
               •закончили
               •контекст уничтожился*/
            using var context = new _41pProductsContext(options);

            /* сооединяем таблицы из базы данных в одну модель продуктов */
            /* тут происходит главный запрос*/
            /*EF Core строит SQL-запрос, но реально отправляет его в БД только когда вы вызываете что-то вроде:
               •ToList()
               •First()
               •Single()
               •Count()*/
            /*include говорит, что кроме основной таблицы нужно подгрузить еще и смежные таблицы*/
            /*IdProductTypeNavigation это целый объект по ключу
             Например, если IdProductType = 2, то IdProductTypeNavigation может содержать:
               new ProductType
               {
                   Id = 2,
                   ProductType1 = "Стол"
               }
               То есть:
               •IdProductType = ключ
               •IdProductTypeNavigation = связанный объект по этому ключу
             */
            var productsFromDb = await context.Products
                .Include(p => p.IdProductTypeNavigation)
                .Include(p => p.IdMaterialTypeNavigation)
                .Include(p => p.ProductWorkshops)
                .ToListAsync();

            /* очищаем нашу ДТО */
            /*чтобы не дублировать карточки, при повторной загрузке*/
            ProductCardItems.Clear();
            
            _allProducts = productsFromDb
                .Select(product => new ProductCardItem
                {
                    Id = product.Id,
                    MinCostPartner = product.MinCostPartner,
                    Article = product.Article,
                    MaterialTypeName = product.IdMaterialTypeNavigation?.MaterialType1 ?? "",
                    ProductionTime = product.ProductWorkshops.Sum(x => x.Time),
                    ProductName = product.Name, 
                    ProductTypeName = product.IdProductTypeNavigation?.ProductType1 ?? "",
                })
                .ToList();
            
            /* очищаем список типов при заполнении*/
            ProductTypes.Clear();
            /*добавляем универсальный вариант, отключающий фильтр*/
            ProductTypes.Add("Все типы");
            
            /*находим все фильтры*/
            var types = _allProducts
                .Select(p => p.ProductTypeName) /*берем только названия типов*/
                .Where(type => !string.IsNullOrWhiteSpace(type)) /*убираем пустые значения*/
                .Distinct() /* оставляем уникальные значения*/
                .OrderBy(type => type) /* сортируем по алфавиту*/
                .ToList();

            foreach (var type in types)
            {
                ProductTypes.Add(type);
            }

            SelectedProductType = "Все типы";
            
            SortOption.Clear();
            SortOption.Add("Без сортировки");
            SortOption.Add("Стоимость по возрастанию");
            SortOption.Add("Стоимость по убыванию");

            SelectedSortOption = "Без сортировки";
            
            UpdateProductList();
            
            // /* заполняем ДТО данными из полной модели продуктов */
            // foreach (var product in productsFromDb)
            // {
            //     /*мы добавляем объект прямо в коллекцию и обсервабл коллекшн уведомляет вью модел о добавлении*/
            //     ProductCardItems.Add(new ProductCardItem
            //     {
            //         Id = product.Id,
            //         MinCostPartner = product.MinCostPartner,
            //         Article = product.Article,
            //         MaterialTypeName = product.IdMaterialTypeNavigation?.MaterialType1 ?? "",
            //         /* Это тоже навигационное свойство, только не одиночное, а коллекция.
            //            У товара может быть несколько связанных записей в таблице product_workshop*/
            //         /*здесь берем все связанные записи ProductWorkshops и суммируете поле Time.
            //            То есть если у товара есть этапы:
            //            •2 часа
            //            •5 часов
            //            •3 часа
            //            то итог будет 10.*/
            //         ProductionTime = product.ProductWorkshops.Sum(x => x.Time),
            //         ProductName = product.Name,
            //         ProductTypeName = product.IdProductTypeNavigation?.ProductType1 ?? "",
            //     });
            // }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task ReloadProductsAsync()
    {
        await LoadProductsAsync();
    }

    public async Task DeleteProductAsync(int productId)
    {
        var options = new DbContextOptionsBuilder<_41pProductsContext>()
            .UseNpgsql("Host=edu.ngknn.ru;Port=5442;Database=41P_products;Username=21P;Password=123")
            .Options;
        
        await using var context = new _41pProductsContext(options);
        
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        
        if (product is null)
            return;
        
        context.Products.Remove(product);
        await context.SaveChangesAsync();
        
        await ReloadProductsAsync();
    }
}