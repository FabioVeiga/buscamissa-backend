﻿// <auto-generated />
using System;
using BuscaMissa.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BuscaMissa.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241116000134_Initial05")]
    partial class Initial05
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("BuscaMissa.Models.CodigoPermissao", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CodigoToken")
                        .HasColumnType("int");

                    b.Property<int?>("ControleId")
                        .HasColumnType("int");

                    b.Property<DateTime>("ValidoAte")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("ControleId");

                    b.ToTable("CodigoPermissoes");
                });

            modelBuilder.Entity("BuscaMissa.Models.Contato", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DDD")
                        .HasColumnType("longtext");

                    b.Property<string>("DDDWhatsApp")
                        .HasColumnType("longtext");

                    b.Property<string>("EmailContato")
                        .HasColumnType("longtext");

                    b.Property<bool?>("EmailContatoValidado")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("IgrejaId")
                        .HasColumnType("int");

                    b.Property<string>("Telefone")
                        .HasColumnType("longtext");

                    b.Property<bool?>("TelefoneValidado")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("TelefoneWhatsApp")
                        .HasColumnType("longtext");

                    b.Property<bool?>("TelefoneWhatsAppValidado")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("IgrejaId");

                    b.ToTable("Contatos");
                });

            modelBuilder.Entity("BuscaMissa.Models.Controle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("IgrejaId")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("IgrejaId");

                    b.ToTable("Controles");
                });

            modelBuilder.Entity("BuscaMissa.Models.Endereco", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Bairro")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Cep")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Complemento")
                        .HasColumnType("longtext");

                    b.Property<string>("Estado")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("IgrejaId")
                        .HasColumnType("int");

                    b.Property<string>("Localidade")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Logradouro")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Regiao")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Uf")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("IgrejaId")
                        .IsUnique();

                    b.ToTable("Enderecos");
                });

            modelBuilder.Entity("BuscaMissa.Models.Igreja", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Alteracao")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("Ativo")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("Criacao")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ImagemUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Paroco")
                        .HasColumnType("longtext");

                    b.Property<int?>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UsuarioId");

                    b.ToTable("Igrejas");
                });

            modelBuilder.Entity("BuscaMissa.Models.IgrejaTemporaria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("IgrejaId")
                        .HasColumnType("int");

                    b.Property<string>("ImagemUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("Paroco")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("IgrejaTemporarias");
                });

            modelBuilder.Entity("BuscaMissa.Models.Missa", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DiaSemana")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("Horario")
                        .HasColumnType("time(6)");

                    b.Property<int>("IgrejaId")
                        .HasColumnType("int");

                    b.Property<string>("Observacao")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("IgrejaId");

                    b.ToTable("Missas");
                });

            modelBuilder.Entity("BuscaMissa.Models.MissaTemporaria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DiaSemana")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("Horario")
                        .HasColumnType("time(6)");

                    b.Property<int>("IgrejaId")
                        .HasColumnType("int");

                    b.Property<string>("Observacao")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("IgrejaId");

                    b.ToTable("MissasTemporarias");
                });

            modelBuilder.Entity("BuscaMissa.Models.RedeSocial", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("IgrejaId")
                        .HasColumnType("int");

                    b.Property<string>("Site")
                        .HasColumnType("longtext");

                    b.Property<int?>("TipoRedeSocial")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("IgrejaId");

                    b.ToTable("RedesSociais");
                });

            modelBuilder.Entity("BuscaMissa.Models.Usuario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool?>("AceitarPromocao")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("AceitarTermo")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("Criacao")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Perfil")
                        .HasColumnType("int");

                    b.Property<string>("Senha")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("BuscaMissa.Models.CodigoPermissao", b =>
                {
                    b.HasOne("BuscaMissa.Models.Controle", "Controle")
                        .WithMany()
                        .HasForeignKey("ControleId");

                    b.Navigation("Controle");
                });

            modelBuilder.Entity("BuscaMissa.Models.Contato", b =>
                {
                    b.HasOne("BuscaMissa.Models.Igreja", "Igreja")
                        .WithMany()
                        .HasForeignKey("IgrejaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Igreja");
                });

            modelBuilder.Entity("BuscaMissa.Models.Controle", b =>
                {
                    b.HasOne("BuscaMissa.Models.Igreja", "Igreja")
                        .WithMany()
                        .HasForeignKey("IgrejaId");

                    b.Navigation("Igreja");
                });

            modelBuilder.Entity("BuscaMissa.Models.Endereco", b =>
                {
                    b.HasOne("BuscaMissa.Models.Igreja", "Igreja")
                        .WithOne("Endereco")
                        .HasForeignKey("BuscaMissa.Models.Endereco", "IgrejaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Igreja");
                });

            modelBuilder.Entity("BuscaMissa.Models.Igreja", b =>
                {
                    b.HasOne("BuscaMissa.Models.Usuario", "Usuario")
                        .WithMany("Igrejas")
                        .HasForeignKey("UsuarioId");

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("BuscaMissa.Models.Missa", b =>
                {
                    b.HasOne("BuscaMissa.Models.Igreja", "Igreja")
                        .WithMany("Missas")
                        .HasForeignKey("IgrejaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Igreja");
                });

            modelBuilder.Entity("BuscaMissa.Models.MissaTemporaria", b =>
                {
                    b.HasOne("BuscaMissa.Models.Igreja", "Igreja")
                        .WithMany()
                        .HasForeignKey("IgrejaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Igreja");
                });

            modelBuilder.Entity("BuscaMissa.Models.RedeSocial", b =>
                {
                    b.HasOne("BuscaMissa.Models.Igreja", "Igreja")
                        .WithMany()
                        .HasForeignKey("IgrejaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Igreja");
                });

            modelBuilder.Entity("BuscaMissa.Models.Igreja", b =>
                {
                    b.Navigation("Endereco")
                        .IsRequired();

                    b.Navigation("Missas");
                });

            modelBuilder.Entity("BuscaMissa.Models.Usuario", b =>
                {
                    b.Navigation("Igrejas");
                });
#pragma warning restore 612, 618
        }
    }
}