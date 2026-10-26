using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("refresh_tokens", PlatformDbContext.Schema);
        builder.HasKey(refreshToken => refreshToken.Id).HasName("pk_refresh_tokens");

        builder.Property(refreshToken => refreshToken.TokenHash)
            .IsRequired()
            .HasMaxLength(RefreshToken.TokenHashLength);

        builder.Property(refreshToken => refreshToken.DeviceId)
            .HasMaxLength(RefreshToken.DeviceIdMaxLength);

        builder.Property(refreshToken => refreshToken.IpAddress)
            .HasMaxLength(RefreshToken.IpAddressMaxLength);

        builder.Property(refreshToken => refreshToken.UserAgent)
            .HasMaxLength(RefreshToken.UserAgentMaxLength);

        builder.Property(refreshToken => refreshToken.RevokedReason)
            .HasMaxLength(RefreshToken.RevokedReasonMaxLength);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.ReplacedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique()
            .HasDatabaseName("uq_refresh_tokens_token_hash");

        builder.HasIndex(refreshToken => new { refreshToken.TenantId, refreshToken.SessionId })
            .HasDatabaseName("ix_refresh_tokens_tenant_id_session_id");

        builder.HasIndex(refreshToken => new { refreshToken.TenantId, refreshToken.UserId })
            .HasDatabaseName("ix_refresh_tokens_tenant_id_user_id");

        builder.HasIndex(refreshToken => new { refreshToken.TenantId, refreshToken.ExpiresAt })
            .HasDatabaseName("ix_refresh_tokens_tenant_id_expires_at");
    }
}
