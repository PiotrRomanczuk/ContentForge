using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRlsPolicies : Migration
    {
        // Tables that need RLS — all app tables created by InitialCreate.
        private static readonly string[] Tables =
        {
            "ContentItems",
            "SocialAccounts",
            "PublishRecords",
            "ContentMetrics",
            "ScheduleConfigs",
            "BotRegistrations"
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in Tables)
            {
                // Enable RLS on the table.
                // In Supabase, the `postgres` role (used by our .NET API) bypasses RLS,
                // but `anon` and `authenticated` roles (used by Supabase REST/client SDK)
                // are subject to it. With no GRANT + no permissive policy, those roles
                // simply get zero rows — locking the table down to backend-only access.
                migrationBuilder.Sql($"ALTER TABLE \"{table}\" ENABLE ROW LEVEL SECURITY;");

                // Force RLS even for the table owner (defense-in-depth).
                // Without this, the table owner bypasses RLS by default.
                migrationBuilder.Sql($"ALTER TABLE \"{table}\" FORCE ROW LEVEL SECURITY;");

                // Explicit policy: only the `postgres` role (our API's service connection) can do anything.
                // This is like a server-side middleware check — no direct client access allowed.
                migrationBuilder.Sql($@"
                    CREATE POLICY ""{table}_service_only"" ON ""{table}""
                    FOR ALL
                    TO postgres
                    USING (true)
                    WITH CHECK (true);
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in Tables)
            {
                migrationBuilder.Sql($"DROP POLICY IF EXISTS \"{table}_service_only\" ON \"{table}\";");
                migrationBuilder.Sql($"ALTER TABLE \"{table}\" NO FORCE ROW LEVEL SECURITY;");
                migrationBuilder.Sql($"ALTER TABLE \"{table}\" DISABLE ROW LEVEL SECURITY;");
            }
        }
    }
}
