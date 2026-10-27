import { ApiError } from "@/lib/http";

export { ApiError };

const STATUS_MESSAGES: Record<number, string> = {
  400: "Dữ liệu gửi lên không hợp lệ.",
  401: "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
  403: "Bạn không có quyền thực hiện thao tác này.",
  404: "Không tìm thấy dữ liệu yêu cầu.",
  409: "Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại.",
  422: "Dữ liệu chưa đúng yêu cầu. Vui lòng kiểm tra lại.",
  423: "Tài khoản đang bị tạm khóa. Vui lòng thử lại sau.",
  429: "Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút.",
  500: "Máy chủ gặp lỗi. Vui lòng thử lại sau.",
  503: "Dịch vụ tạm thời không khả dụng. Vui lòng thử lại sau.",
};

const CODE_MESSAGES: Record<string, string> = {
  AUTH_EMAIL_EXISTS: "Email này đã được sử dụng.",
  AUTH_INVALID_CREDENTIALS: "Email hoặc mật khẩu không đúng.",
  AUTH_TOKEN_EXPIRED: "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
  AUTH_TOKEN_INVALID: "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.",
  AUTH_REFRESH_TOKEN_EXPIRED: "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
  AUTH_REFRESH_TOKEN_REVOKED: "Phiên đăng nhập đã bị thu hồi. Vui lòng đăng nhập lại.",
  AUTH_ACCOUNT_DISABLED: "Tài khoản của bạn đã bị khóa.",
  RECIPE_NOT_FOUND: "Không tìm thấy công thức.",
  RECIPE_SLUG_EXISTS: "Đường dẫn công thức đã tồn tại.",
  RECIPE_PUBLISH_INCOMPLETE:
    "Cần thêm ít nhất một bước thực hiện trước khi xuất bản.",
  RECIPE_FORBIDDEN: "Bạn không phải tác giả của công thức này.",
  RECIPE_CONCURRENCY_CONFLICT:
    "Công thức đã bị người khác cập nhật. Vui lòng tải lại bản mới.",
  CATEGORY_NOT_FOUND: "Không tìm thấy danh mục.",
  CATEGORY_NAME_EXISTS: "Tên danh mục đã tồn tại.",
  CATEGORY_DELETE_HAS_RECIPES: "Danh mục vẫn còn công thức, không thể xóa.",
  FILE_SIZE_EXCEEDED: "Tệp vượt quá dung lượng cho phép (5MB).",
  FILE_MIME_INVALID: "Định dạng tệp không được hỗ trợ.",
  RATE_LIMIT_EXCEEDED: "Bạn thao tác quá nhanh. Vui lòng thử lại sau.",
  VALIDATION_ERROR: "Dữ liệu chưa hợp lệ. Vui lòng kiểm tra lại.",
};

export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError;
}

export function isUnauthorized(error: unknown) {
  return isApiError(error) && error.status === 401;
}

export function isForbidden(error: unknown) {
  return isApiError(error) && error.status === 403;
}

export function isNotFound(error: unknown) {
  return isApiError(error) && error.status === 404;
}

export function isConflict(error: unknown) {
  return isApiError(error) && error.status === 409;
}

export function isValidationError(error: unknown) {
  return isApiError(error) && error.status === 422;
}

/** Lấy message thân thiện từ ApiError / Error / unknown. */
export function getErrorMessage(
  error: unknown,
  fallback = "Có lỗi xảy ra. Vui lòng thử lại."
) {
  if (isApiError(error)) {
    if (error.code && CODE_MESSAGES[error.code]) return CODE_MESSAGES[error.code];
    if (error.detail) return error.detail;
    if (error.message && error.message !== "Failed to fetch") return error.message;
    return STATUS_MESSAGES[error.status] ?? fallback;
  }
  if (error instanceof Error) {
    if (error.message === "Failed to fetch")
      return "Không kết nối được máy chủ. Vui lòng kiểm tra Backend.";
    return error.message || fallback;
  }
  return fallback;
}

/** Lấy lỗi theo field để hiển thị inline cạnh input (NFR-USE-003). */
export function getFieldErrors(
  error: unknown
): Record<string, string[]> {
  return isApiError(error) && error.errors ? error.errors : {};
}

export function getFirstFieldError(
  error: unknown,
  field: string
): string | undefined {
  return getFieldErrors(error)[field]?.[0];
}
