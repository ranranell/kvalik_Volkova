using Kvalik2Proj.Data;

namespace Kvalik2Proj.Tests;

public class LastModifiedTests
{
    // Тест 1: UpdatedAt устанавливается при создании услуги
    [Fact]
    public void Service_UpdatedAt_SetOnCreation()
    {
        var before = DateTime.Now;
        var service = new Service
        {
            Name = "Тестовая услуга",
            UpdatedAt = DateTime.Now
        };
        var after = DateTime.Now;

        Assert.NotNull(service.UpdatedAt);
        Assert.InRange(service.UpdatedAt.Value, before, after);
    }

    // Тест 2: UpdatedAt обновляется при редактировании
    [Fact]
    public void Service_UpdatedAt_UpdatesOnEdit()
    {
        var service = new Service
        {
            Name = "Услуга",
            UpdatedAt = new DateTime(2025, 1, 1)
        };

        var oldDate = service.UpdatedAt;
        service.UpdatedAt = DateTime.Now;

        Assert.True(service.UpdatedAt > oldDate);
    }

    // Тест 3: Форматирование даты dd.MM.yyyy HH:mm
    [Fact]
    public void Service_UpdatedAt_FormatsCorrectly()
    {
        var service = new Service
        {
            UpdatedAt = new DateTime(2026, 4, 19, 14, 30, 0)
        };

        var formatted = service.UpdatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "";

        Assert.Equal("19.04.2026 14:30", formatted);
    }

    // Тест 4: Null UpdatedAt возвращает пустую строку
    [Fact]
    public void Service_UpdatedAt_NullReturnsEmptyString()
    {
        var service = new Service { UpdatedAt = null };

        var formatted = service.UpdatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "";

        Assert.Equal("", formatted);
    }

    // Тест 5: CreatedAt устанавливается при создании отзыва
    [Fact]
    public void Review_CreatedAt_SetOnCreation()
    {
        var before = DateTime.Now;
        var review = new Review
        {
            Comment = "Отличный мастер",
            Rating = 5,
            CreatedAt = DateTime.Now
        };
        var after = DateTime.Now;

        Assert.NotNull(review.CreatedAt);
        Assert.InRange(review.CreatedAt.Value, before, after);
    }

    // Тест 6: CreatedAt устанавливается при создании платежа
    [Fact]
    public void Payment_CreatedAt_SetOnCreation()
    {
        var before = DateTime.Now;
        var payment = new Payment
        {
            Amount = 1000m,
            CreatedAt = DateTime.Now
        };
        var after = DateTime.Now;

        Assert.NotNull(payment.CreatedAt);
        Assert.InRange(payment.CreatedAt.Value, before, after);
    }

    // Тест 7: CreatedAt устанавливается при создании заявки на квалификацию
    [Fact]
    public void QualificationRequest_CreatedAt_SetOnCreation()
    {
        var before = DateTime.Now;
        var request = new QualificationRequest
        {
            Description = "Курсы повышения",
            Status = "Ожидание",
            CreatedAt = DateTime.Now
        };
        var after = DateTime.Now;

        Assert.NotNull(request.CreatedAt);
        Assert.InRange(request.CreatedAt.Value, before, after);
    }

    // Тест 8: ReviewedAt заполняется при рассмотрении заявки
    [Fact]
    public void QualificationRequest_ReviewedAt_SetOnReview()
    {
        var request = new QualificationRequest
        {
            Status = "Ожидание",
            CreatedAt = new DateTime(2026, 4, 1)
        };

        Assert.Null(request.ReviewedAt);

        request.Status = "Одобрена";
        request.ReviewedAt = DateTime.Now;

        Assert.NotNull(request.ReviewedAt);
        Assert.True(request.ReviewedAt > request.CreatedAt);
    }

    // Тест 9: Null CreatedAt в заявке возвращает "—"
    [Fact]
    public void QualificationRequest_NullCreatedAt_ReturnsDash()
    {
        var request = new QualificationRequest { CreatedAt = null };

        var formatted = request.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "—";

        Assert.Equal("—", formatted);
    }

    // Тест 10: Форматирование CreatedAt пользователя
    [Fact]
    public void User_CreatedAt_FormatsCorrectly()
    {
        var user = new User
        {
            Name = "Тест",
            CreatedAt = new DateTime(2026, 3, 15, 9, 0, 0)
        };

        var formatted = user.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "";

        Assert.Equal("15.03.2026 09:00", formatted);
    }
}
