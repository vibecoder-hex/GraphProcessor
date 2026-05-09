using System;
using System.Collections.Generic;
using GraphProcessorAPI.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GraphProcessorAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:accountrole", "admin,user")
                .Annotation("Npgsql:Enum:accountrole.user_role", "user,admin")
                .Annotation("Npgsql:Enum:algorithm_type", "bfs,dfs,dijkstra")
                .Annotation("Npgsql:Enum:algorithm_type.algorithm_type", "dfs,bfs,dijkstra")
                .Annotation("Npgsql:Enum:graphtype", "non_oriented,oriented")
                .Annotation("Npgsql:Enum:graphtype.graph_type", "oriented,non_oriented");

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    first_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    last_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateOnly>(type: "date", nullable: false),
                    email = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    password_hash = table.Column<string>(type: "character(256)", fixedLength: true, maxLength: 256, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    role = table.Column<UserRole>(type: "accountrole", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_id_pk", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "graph",
                columns: table => new
                {
                    graph_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    creationat = table.Column<DateOnly>(type: "date", nullable: false),
                    structure = table.Column<string>(type: "json", nullable: false),
                    type = table.Column<GraphType>(type: "graphtype", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("graph_id_pk", x => x.graph_id);
                    table.ForeignKey(
                        name: "graph__fk",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "edge",
                columns: table => new
                {
                    edge_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    description = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    weight = table.Column<int>(type: "integer", nullable: false),
                    graph_id = table.Column<int>(type: "integer", nullable: false),
                    from_node = table.Column<int>(type: "integer", nullable: false),
                    to_node = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edge_id_pk", x => x.edge_id);
                    table.ForeignKey(
                        name: "edge_graph_id__fk",
                        column: x => x.graph_id,
                        principalTable: "graph",
                        principalColumn: "graph_id");
                });

            migrationBuilder.CreateTable(
                name: "node",
                columns: table => new
                {
                    node_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    image = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    graph_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("node_id_pk", x => x.node_id);
                    table.ForeignKey(
                        name: "node_graph_id__fk",
                        column: x => x.graph_id,
                        principalTable: "graph",
                        principalColumn: "graph_id");
                });

            migrationBuilder.CreateTable(
                name: "processing_result",
                columns: table => new
                {
                    processing_result_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    graph_id = table.Column<int>(type: "integer", nullable: false),
                    time_in_ns = table.Column<double>(type: "double precision", nullable: false),
                    start_vertex = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    target_vertex = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    shortest_path = table.Column<List<string>>(type: "character varying(40)[]", nullable: false),
                    total_distance = table.Column<int>(type: "integer", nullable: true),
                    algorithm = table.Column<AlgorithmType>(type: "algorithm_type", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("processing_result_id_pk", x => x.processing_result_id);
                    table.ForeignKey(
                        name: "processing_result__fk",
                        column: x => x.graph_id,
                        principalTable: "graph",
                        principalColumn: "graph_id");
                });

            migrationBuilder.CreateIndex(
                name: "edge_fk_index",
                table: "edge",
                column: "graph_id");

            migrationBuilder.CreateIndex(
                name: "edge_pk",
                table: "edge",
                columns: new[] { "value", "from_node", "to_node" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "graph__fk_index",
                table: "graph",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "graph_unq",
                table: "graph",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "node_fk_index",
                table: "node",
                column: "graph_id");

            migrationBuilder.CreateIndex(
                name: "processing_result__fk_index",
                table: "processing_result",
                column: "graph_id");

            migrationBuilder.CreateIndex(
                name: "user_unq",
                table: "user",
                columns: new[] { "username", "email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "edge");

            migrationBuilder.DropTable(
                name: "node");

            migrationBuilder.DropTable(
                name: "processing_result");

            migrationBuilder.DropTable(
                name: "graph");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
