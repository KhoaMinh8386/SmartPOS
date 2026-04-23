using System;
using SmartPos.Module.Promotions.Backend;
using SmartPos.Module.Promotions.Models;

namespace SmartPos.Module.Promotions.Controllers
{
    public class PromotionController
    {
        private readonly PromotionBackend _backend;

        public PromotionController()
        {
            _backend = new PromotionBackend();
            _backend.EnsureSchema();
        }

        // ─────────────────────────────────────────────
        //  Load
        // ─────────────────────────────────────────────
        public PromotionDataBundle LoadData()
        {
            return _backend.LoadData();
        }

        // ─────────────────────────────────────────────
        //  Voucher CRUD
        // ─────────────────────────────────────────────
        public void SaveVoucher(VoucherItem voucher)
        {
            ValidateVoucher(voucher);

            // Kiểm tra trùng mã voucher (chỉ khi thêm mới hoặc thay đổi code)
            if (_backend.IsVoucherCodeDuplicate(voucher.VoucherCode, voucher.VoucherID))
            {
                throw new InvalidOperationException(
                    $"Mã voucher '{voucher.VoucherCode}' đã tồn tại. Vui lòng nhập mã khác.");
            }

            _backend.SaveVoucher(voucher);
        }

        public void DeleteVoucher(int voucherId)
        {
            if (voucherId <= 0)
            {
                throw new InvalidOperationException("Vui lòng chọn voucher cần xóa.");
            }

            _backend.DeleteVoucher(voucherId);
        }

        // ─────────────────────────────────────────────
        //  ProductSale CRUD
        // ─────────────────────────────────────────────
        public void SaveProductSale(ProductSaleItem sale)
        {
            ValidateSale(sale);
            _backend.SaveProductSale(sale);
        }

        public void DeleteProductSale(int saleId)
        {
            if (saleId <= 0)
            {
                throw new InvalidOperationException("Vui lòng chọn chương trình sale cần xóa.");
            }

            _backend.DeleteProductSale(saleId);
        }

        // ─────────────────────────────────────────────
        //  Preview — Quy tắc ưu tiên AllowStackDiscount
        // ─────────────────────────────────────────────
        /// <summary>
        /// Tính toán kết quả áp dụng khuyến mãi theo quy tắc:
        ///   1. Cả hai cho phép stack → áp đồng thời (Sale trước, Voucher trên giá còn lại)
        ///   2. Không stack → so sánh Priority (số nhỏ = ưu tiên cao)
        ///   3. Priority bằng nhau → chọn mức giảm lớn hơn
        /// </summary>
        public PromotionPreviewResult Preview(PromotionPreviewRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("Dữ liệu xem trước không hợp lệ.");
            }

            decimal saleDiscount    = ComputeSaleDiscount(request.ProductSale, request.ProductAmount);
            decimal voucherDiscount = ComputeVoucherDiscount(request.Voucher, request.OrderAmount);

            bool canStack = request.ProductSale != null
                         && request.Voucher     != null
                         && request.ProductSale.AllowStackVoucher
                         && request.Voucher.AllowStackDiscount;

            var result = new PromotionPreviewResult();

            if (canStack)
            {
                // Áp Sale trước → tính Voucher trên số tiền còn lại
                result.SaleDiscount    = saleDiscount;
                decimal afterSale      = request.OrderAmount - saleDiscount;
                result.VoucherDiscount = ComputeVoucherDiscount(request.Voucher, afterSale);
                result.TotalDiscount   = result.SaleDiscount + result.VoucherDiscount;
                result.StackMode       = true;
                result.AppliedRule     = "✅ Áp dụng ĐỒNG THỜI Sale + Voucher (AllowStackDiscount = true).";
                result.PriorityExplanation =
                    "Cả hai chương trình đều bật AllowStack → không cần loại trừ theo Priority.\n" +
                    "Sale được trừ trước, Voucher áp dụng trên số tiền còn lại.";
            }
            else
            {
                int salePriority    = request.ProductSale == null ? int.MaxValue : request.ProductSale.Priority;
                int voucherPriority = request.Voucher     == null ? int.MaxValue : request.Voucher.Priority;

                if (salePriority < voucherPriority)
                {
                    result.SaleDiscount    = saleDiscount;
                    result.TotalDiscount   = saleDiscount;
                    result.StackMode       = false;
                    result.AppliedRule     = "🔶 Chỉ áp dụng Sale sản phẩm.";
                    result.PriorityExplanation =
                        $"Sale Priority = {salePriority} < Voucher Priority = {voucherPriority} → Sale thắng.";
                }
                else if (voucherPriority < salePriority)
                {
                    result.VoucherDiscount = voucherDiscount;
                    result.TotalDiscount   = voucherDiscount;
                    result.StackMode       = false;
                    result.AppliedRule     = "🔶 Chỉ áp dụng Voucher.";
                    result.PriorityExplanation =
                        $"Voucher Priority = {voucherPriority} < Sale Priority = {salePriority} → Voucher thắng.";
                }
                else
                {
                    // Cùng Priority → chọn mức giảm lớn hơn (có lợi cho khách)
                    if (saleDiscount >= voucherDiscount)
                    {
                        result.SaleDiscount    = saleDiscount;
                        result.TotalDiscount   = saleDiscount;
                        result.StackMode       = false;
                        result.AppliedRule     = "🔶 Chỉ áp dụng Sale sản phẩm.";
                        result.PriorityExplanation =
                            $"Priority bằng nhau ({salePriority}) → hệ thống chọn mức giảm LỚN HƠN (Sale: {saleDiscount:N0} ≥ Voucher: {voucherDiscount:N0}).";
                    }
                    else
                    {
                        result.VoucherDiscount = voucherDiscount;
                        result.TotalDiscount   = voucherDiscount;
                        result.StackMode       = false;
                        result.AppliedRule     = "🔶 Chỉ áp dụng Voucher.";
                        result.PriorityExplanation =
                            $"Priority bằng nhau ({voucherPriority}) → hệ thống chọn mức giảm LỚN HƠN (Voucher: {voucherDiscount:N0} > Sale: {saleDiscount:N0}).";
                    }
                }
            }

            result.FinalAmount = request.OrderAmount - result.TotalDiscount;
            if (result.FinalAmount < 0)
            {
                result.FinalAmount = 0;
            }

            return result;
        }

        // ─────────────────────────────────────────────
        //  Private — Tính toán
        // ─────────────────────────────────────────────
        private static decimal ComputeSaleDiscount(ProductSaleItem sale, decimal productAmount)
        {
            if (sale == null || !sale.IsActive)
            {
                return 0m;
            }

            DateTime now = DateTime.Now;
            if (now < sale.StartDate || now > sale.EndDate)
            {
                return 0m;
            }

            // Ưu tiên SalePrice (giá cố định) nếu được khai báo
            if (sale.SalePrice.HasValue && sale.SalePrice.Value > 0)
            {
                decimal discount = productAmount - sale.SalePrice.Value;
                return discount > 0 ? discount : 0m;
            }

            // DiscountType 1 = %
            if (sale.DiscountType == 1)
            {
                return productAmount * sale.DiscountValue / 100m;
            }

            // DiscountType 2 = Số tiền cố định
            if (sale.DiscountType == 2)
            {
                return sale.DiscountValue > productAmount ? productAmount : sale.DiscountValue;
            }

            return 0m;
        }

        private static decimal ComputeVoucherDiscount(VoucherItem voucher, decimal orderAmount)
        {
            if (voucher == null || !voucher.IsActive)
            {
                return 0m;
            }

            DateTime now = DateTime.Now;
            if (now < voucher.StartDate || now > voucher.EndDate)
            {
                return 0m;
            }

            if (orderAmount < voucher.MinOrderValue)
            {
                return 0m;
            }

            decimal discount;
            if (voucher.DiscountType == 1)
            {
                discount = orderAmount * voucher.DiscountValue / 100m;
            }
            else if (voucher.DiscountType == 2)
            {
                discount = voucher.DiscountValue;
            }
            else
            {
                discount = 0m;
            }

            // Giới hạn MaxDiscount nếu có
            if (voucher.MaxDiscount.HasValue && discount > voucher.MaxDiscount.Value)
            {
                discount = voucher.MaxDiscount.Value;
            }

            // Không giảm quá tổng đơn
            if (discount > orderAmount)
            {
                discount = orderAmount;
            }

            return discount;
        }

        // ─────────────────────────────────────────────
        //  Private — Validation
        // ─────────────────────────────────────────────
        private static void ValidateVoucher(VoucherItem voucher)
        {
            if (voucher == null)
            {
                throw new InvalidOperationException("Dữ liệu voucher không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(voucher.VoucherCode) && voucher.VoucherID <= 0)
            {
                throw new InvalidOperationException("Vui lòng nhập mã voucher.");
            }

            if (voucher.DiscountValue <= 0)
            {
                throw new InvalidOperationException("Giá trị giảm voucher phải lớn hơn 0.");
            }

            if (voucher.DiscountType == 1 && voucher.DiscountValue > 100)
            {
                throw new InvalidOperationException("Giảm theo % không được vượt quá 100%.");
            }

            if (voucher.EndDate <= voucher.StartDate)
            {
                throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu.");
            }

            if (voucher.Priority <= 0)
            {
                throw new InvalidOperationException("Priority phải lớn hơn 0.");
            }
        }

        private static void ValidateSale(ProductSaleItem sale)
        {
            if (sale == null)
            {
                throw new InvalidOperationException("Dữ liệu sale sản phẩm không hợp lệ.");
            }

            if (sale.ProductID <= 0)
            {
                throw new InvalidOperationException("Vui lòng chọn sản phẩm áp dụng sale.");
            }

            if (string.IsNullOrWhiteSpace(sale.SaleName))
            {
                throw new InvalidOperationException("Vui lòng nhập tên chương trình sale.");
            }

            // Phải có ít nhất một trong hai: DiscountValue hoặc SalePrice
            bool hasDiscountValue = sale.DiscountValue > 0;
            bool hasSalePrice     = sale.SalePrice.HasValue && sale.SalePrice.Value > 0;

            if (!hasDiscountValue && !hasSalePrice)
            {
                throw new InvalidOperationException(
                    "Vui lòng nhập Giá trị giảm hoặc Giá sale cố định.");
            }

            if (sale.DiscountType == 1 && sale.DiscountValue > 100)
            {
                throw new InvalidOperationException("Giảm theo % không được vượt quá 100%.");
            }

            if (sale.EndDate <= sale.StartDate)
            {
                throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu.");
            }

            if (sale.Priority <= 0)
            {
                throw new InvalidOperationException("Priority phải lớn hơn 0.");
            }
        }
    }
}
