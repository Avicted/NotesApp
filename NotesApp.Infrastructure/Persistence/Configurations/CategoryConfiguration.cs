using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotesApp.Domain.Entities;

namespace NotesApp.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Created)
            .IsRequired();

        builder.Property(c => c.LastModified)
            .IsRequired();

        builder.HasMany(c => c.Notes)
            .WithOne(n => n.Category)
            .HasForeignKey(n => n.CategoryId);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.HasOne(n => n.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(n => n.UserId)
            .IsRequired();
    }
}
