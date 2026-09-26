using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
///     Model builder extensions for the database context
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Applies snake_case naming convention to the model.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    public static void UseSnakeCaseNamingConvention(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
                entity.SetTableName(tableName.ToPlural().ToSnakeCase()); 
            
            foreach (var property in entity.GetProperties())
            {
                if (property.IsPrimaryKey() || property.IsForeignKey()) continue;
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrEmpty(keyName))
                    key.SetName(keyName.ToSnakeCase());
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var foreignKeyName = foreignKey.GetConstraintName();
                if (!string.IsNullOrEmpty(foreignKeyName))
                    foreignKey.SetConstraintName(foreignKeyName.ToSnakeCase());
            }

            foreach (var index in entity.GetIndexes())
            {
                var indexDatabaseName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexDatabaseName))
                    index.SetDatabaseName(indexDatabaseName.ToSnakeCase());
            }
        }
    }
}