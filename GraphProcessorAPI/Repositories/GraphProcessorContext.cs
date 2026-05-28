using GraphProcessorAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphProcessorAPI.Repositories;

public partial class GraphProcessorContext : DbContext
{

    public GraphProcessorContext(DbContextOptions<GraphProcessorContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Edge> Edges { get; set; }

    public virtual DbSet<Graph> Graphs { get; set; }

    public virtual DbSet<Node> Nodes { get; set; }

    public virtual DbSet<ProcessingResult> ProcessingResults { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<UserRole>("accountrole")
            .HasPostgresEnum<AlgorithmType>("algorithm_type")
            .HasPostgresEnum<GraphType>("graphtype");

        modelBuilder.Entity<Edge>(entity =>
        {
            entity.HasKey(e => e.EdgeId).HasName("edge_id_pk");

            entity.ToTable("edge");

            entity.HasIndex(e => e.GraphId, "edge_fk_index");

            entity.HasIndex(e => new { e.Value, e.FromNode, e.ToNode }, "edge_pk").IsUnique();

            entity.Property(e => e.EdgeId).HasColumnName("edge_id");
            entity.Property(e => e.Description)
                .HasMaxLength(80)
                .HasColumnName("description");
            entity.Property(e => e.FromNode).HasColumnName("from_node");
            entity.Property(e => e.GraphId).HasColumnName("graph_id");
            entity.Property(e => e.ToNode).HasColumnName("to_node");
            entity.Property(e => e.Value)
                .HasMaxLength(40)
                .HasColumnName("value");
            entity.Property(e => e.Weight).HasColumnName("weight");

            entity.HasOne(d => d.Graph).WithMany(p => p.Edges)
                .HasForeignKey(d => d.GraphId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("edge_graph_id__fk");
        });

        modelBuilder.Entity<Graph>(entity =>
        {
            entity.HasKey(e => e.GraphId).HasName("graph_id_pk");

            entity.ToTable("graph");

            entity.HasIndex(e => e.UserId, "graph__fk_index");

            entity.HasIndex(e => e.Name, "graph_unq").IsUnique();

            entity.Property(e => e.GraphId).HasColumnName("graph_id");
            entity.Property(e => e.Creationat).HasColumnName("creationat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(40)
                .HasColumnName("name");
            entity.Property(e => e.Structure)
                .HasColumnType("json")
                .HasColumnName("structure");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasColumnType("graphtype");

            entity.HasOne(d => d.User).WithMany(p => p.Graphs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("graph__fk");
        });

        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(e => e.NodeId).HasName("node_id_pk");

            entity.ToTable("node");

            entity.HasIndex(e => e.GraphId, "node_fk_index");

            entity.Property(e => e.NodeId).HasColumnName("node_id");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasColumnName("color");
            entity.Property(e => e.Description)
                .HasMaxLength(80)
                .HasColumnName("description");
            entity.Property(e => e.GraphId).HasColumnName("graph_id");
            entity.Property(e => e.Image)
                .HasMaxLength(256)
                .HasColumnName("image");
            entity.Property(e => e.Value)
                .HasMaxLength(40)
                .HasColumnName("value");

            entity.HasOne(d => d.Graph).WithMany(p => p.Nodes)
                .HasForeignKey(d => d.GraphId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("node_graph_id__fk");
        });

        modelBuilder.Entity<ProcessingResult>(entity =>
        {
            entity.HasKey(e => e.ProcessingResultId).HasName("processing_result_id_pk");

            entity.ToTable("processing_result");

            entity.HasIndex(e => e.GraphId, "processing_result__fk_index");

            entity.Property(e => e.ProcessingResultId).HasColumnName("processing_result_id");
            entity.Property(e => e.GraphId).HasColumnName("graph_id");
            entity.Property(e => e.ShortestPath)
                .HasColumnType("character varying(40)[]")
                .HasColumnName("shortest_path");
            entity.Property(e => e.StartVertex)
                .HasMaxLength(40)
                .HasColumnName("start_vertex");
            entity.Property(e => e.TargetVertex)
                .HasMaxLength(40)
                .HasColumnName("target_vertex");
            entity.Property(e => e.TimeInNs).HasColumnName("time_in_ns");
            entity.Property(e => e.TotalDistance).HasColumnName("total_distance");
            entity.Property(e => e.Algorithm)
                .HasColumnName("algorithm")
                .HasColumnType("algorithm_type");
            entity.HasOne(d => d.Graph).WithMany(p => p.ProcessingResults)
                .HasForeignKey(d => d.GraphId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("processing_result__fk");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("user_id_pk");

            entity.ToTable("user");

            entity.HasIndex(e => new { e.Username, e.Email }, "user_unq").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(80)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(20)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(20)
                .HasColumnName("last_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .IsFixedLength()
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Username)
                .HasMaxLength(40)
                .HasColumnName("username");
            entity.Property(e => e.Role)
                .HasColumnName("role")
                .HasColumnType("accountrole");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
