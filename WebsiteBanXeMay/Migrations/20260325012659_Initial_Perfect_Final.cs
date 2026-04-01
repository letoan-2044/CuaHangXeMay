using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBanXeMay.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Perfect_Final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanPham_DanhMuc_MaDanhMuc",
                table: "SanPham");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiKhoan_ChucVu_MaChucVu",
                table: "TaiKhoan");

            migrationBuilder.AlterColumn<int>(
                name: "MaChucVu",
                table: "TaiKhoan",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "DanhMucMaDanhMuc",
                table: "SanPham",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "DanhMuc",
                columns: new[] { "MaDanhMuc", "MoTa", "TenDanhMuc" },
                values: new object[] { 4, null, "Xe điện" });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "MaSanPham",
                keyValue: 1,
                columns: new[] { "DanhMucMaDanhMuc", "HinhAnh", "MoTa", "TenSanPham" },
                values: new object[] { null, "wave-alpha.jpg", "Xe số phổ thông tiết kiệm nhiên liệu", "Honda Wave Alpha 110" });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "MaSanPham",
                keyValue: 2,
                columns: new[] { "DanhMucMaDanhMuc", "MoTa", "TenSanPham" },
                values: new object[] { null, "Xe ga thời trang, vận hành êm ái", "Honda Air Blade 125" });

            migrationBuilder.InsertData(
                table: "SanPham",
                columns: new[] { "MaSanPham", "DanhMucMaDanhMuc", "Gia", "HinhAnh", "MaDanhMuc", "MoTa", "SoLuongTon", "TenSanPham", "TrangThai" },
                values: new object[] { 3, null, 48900000m, "exciter.jpg", 1, "Xe côn tay thể thao mạnh mẽ", 20, "Yamaha Exciter 155", true });

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 1,
                column: "SoDienThoai",
                value: "0901234567");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 2,
                column: "HoTen",
                value: "Nhân Viên Bán Hàng");

            migrationBuilder.InsertData(
                table: "TaiKhoan",
                columns: new[] { "MaTaiKhoan", "DiaChi", "HoTen", "MaChucVu", "MatKhau", "SoDienThoai", "TenDangNhap", "TrangThai" },
                values: new object[] { 3, "An Giang", "Khách Hàng Test", 3, "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=", "0912345678", "khachhang", true });

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_DanhMucMaDanhMuc",
                table: "SanPham",
                column: "DanhMucMaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_TrangThai",
                table: "SanPham",
                column: "TrangThai");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPham_DanhMuc",
                table: "SanPham",
                column: "MaDanhMuc",
                principalTable: "DanhMuc",
                principalColumn: "MaDanhMuc",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SanPham_DanhMuc_DanhMucMaDanhMuc",
                table: "SanPham",
                column: "DanhMucMaDanhMuc",
                principalTable: "DanhMuc",
                principalColumn: "MaDanhMuc");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiKhoan_ChucVu",
                table: "TaiKhoan",
                column: "MaChucVu",
                principalTable: "ChucVu",
                principalColumn: "MaChucVu",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanPham_DanhMuc",
                table: "SanPham");

            migrationBuilder.DropForeignKey(
                name: "FK_SanPham_DanhMuc_DanhMucMaDanhMuc",
                table: "SanPham");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiKhoan_ChucVu",
                table: "TaiKhoan");

            migrationBuilder.DropIndex(
                name: "IX_SanPham_DanhMucMaDanhMuc",
                table: "SanPham");

            migrationBuilder.DropIndex(
                name: "IX_SanPham_TrangThai",
                table: "SanPham");

            migrationBuilder.DeleteData(
                table: "DanhMuc",
                keyColumn: "MaDanhMuc",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SanPham",
                keyColumn: "MaSanPham",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "DanhMucMaDanhMuc",
                table: "SanPham");

            migrationBuilder.AlterColumn<int>(
                name: "MaChucVu",
                table: "TaiKhoan",
                type: "int",
                nullable: false,
                defaultValue: 3,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "MaSanPham",
                keyValue: 1,
                columns: new[] { "HinhAnh", "MoTa", "TenSanPham" },
                values: new object[] { "wave.jpg", null, "Wave Alpha 110" });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "MaSanPham",
                keyValue: 2,
                columns: new[] { "MoTa", "TenSanPham" },
                values: new object[] { null, "Air Blade 125" });

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 1,
                column: "SoDienThoai",
                value: "0123456789");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 2,
                column: "HoTen",
                value: "Nhân Viên");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPham_DanhMuc_MaDanhMuc",
                table: "SanPham",
                column: "MaDanhMuc",
                principalTable: "DanhMuc",
                principalColumn: "MaDanhMuc",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiKhoan_ChucVu_MaChucVu",
                table: "TaiKhoan",
                column: "MaChucVu",
                principalTable: "ChucVu",
                principalColumn: "MaChucVu",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
