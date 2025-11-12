using System;
using System.Collections.Generic;
using TicketBookingStaffApp.DAL.repositories;
using TicketBookingStaffApp.Models;
using TicketBookingStaffApp.Utils;

namespace TicketBookingStaffApp.BLL
{
    public class movieBLL
    {
        private readonly movieRepository _movieRepository = new movieRepository();

        /// <summary>
        /// Lấy danh sách phim theo trang (có thể kèm logic xử lý trước khi trả về UI)
        /// </summary>
        public List<Movie> GetMovies(int page = 1, int pageSize = 10)
        {
            // Gọi đến repository để lấy dữ liệu
            List<Movie> movies = _movieRepository.GetAllMovies(page, pageSize);

            // (Tùy chọn) Có thể xử lý thêm, ví dụ: cắt synopsis quá dài, định dạng ngày
            foreach (var movie in movies)
            {
                if (!string.IsNullOrEmpty(movie.Synopsis) && movie.Synopsis.Length > 100)
                {
                    movie.Synopsis = movie.Synopsis.Substring(0, 100) + "...";
                }
            }

            return movies;
        }

        /// <summary>
        /// Lấy tổng số trang dựa trên tổng số phim và kích thước trang
        /// </summary>
        public int GetTotalPages(int pageSize)
        {
            int totalMovies = _movieRepository.GetTotalMovies();
            return Paginations.CalculateTotalPages(totalMovies, pageSize);
        }

        /// <summary>
        /// Lấy tổng số phim (nếu cần)
        /// </summary>
        public int GetTotalMovies()
        {
            return _movieRepository.GetTotalMovies();
        }

        /// <summary>
        /// Lấy chi tiết phim theo ID (BLL gọi Repository)
        /// </summary>
        public Movie GetMovieDetail(string movieId)
        {
            return _movieRepository.GetMovieById(movieId);
        }

    }
}
