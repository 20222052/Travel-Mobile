import { useState, useEffect } from 'react';

/**
 * Hook để quản lý breadcrumb trong admin
 */
export const useBreadcrumb = (items = []) => {
  const [breadcrumbs, setBreadcrumbs] = useState(items);

  return { breadcrumbs, setBreadcrumbs };
};

/**
 * Hook để quản lý page title
 */
export const usePageTitle = (title) => {
  useEffect(() => {
    document.title = `${title} - Travel Admin`;
    return () => {
      document.title = 'Travel Admin';
    };
  }, [title]);
};

/**
 * Hook để kiểm tra permissions
 */
export const usePermission = (requiredPermission) => {
  const [hasPermission, setHasPermission] = useState(false);

  useEffect(() => {
    const userPermissions = JSON.parse(localStorage.getItem('adminPermissions') || '[]');
    setHasPermission(userPermissions.includes(requiredPermission) || userPermissions.includes('*'));
  }, [requiredPermission]);

  return hasPermission;
};

/**
 * Hook để fetch data với loading và error states
 */
export const useAdminFetch = (fetchFn, dependencies = []) => {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const refetch = async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await fetchFn();
      setData(result);
    } catch (err) {
      setError(err.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refetch();
  }, dependencies);

  return { data, loading, error, refetch };
};

/**
 * Hook để quản lý table pagination
 */
export const usePagination = (initialPage = 1, initialPageSize = 10) => {
  const [currentPage, setCurrentPage] = useState(initialPage);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const [totalItems, setTotalItems] = useState(0);

  const totalPages = Math.ceil(totalItems / pageSize);

  const goToPage = (page) => {
    if (page >= 1 && page <= totalPages) {
      setCurrentPage(page);
    }
  };

  const nextPage = () => goToPage(currentPage + 1);
  const prevPage = () => goToPage(currentPage - 1);
  const firstPage = () => goToPage(1);
  const lastPage = () => goToPage(totalPages);

  return {
    currentPage,
    pageSize,
    totalItems,
    totalPages,
    setCurrentPage,
    setPageSize,
    setTotalItems,
    goToPage,
    nextPage,
    prevPage,
    firstPage,
    lastPage
  };
};

/**
 * Hook để quản lý search và filter
 */
export const useTableFilters = (initialFilters = {}) => {
  const [filters, setFilters] = useState(initialFilters);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('');
  const [sortOrder, setSortOrder] = useState('asc');

  const updateFilter = (key, value) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  };

  const clearFilters = () => {
    setFilters(initialFilters);
    setSearchTerm('');
  };

  const toggleSort = (field) => {
    if (sortBy === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(field);
      setSortOrder('asc');
    }
  };

  return {
    filters,
    searchTerm,
    sortBy,
    sortOrder,
    setFilters,
    setSearchTerm,
    setSortBy,
    setSortOrder,
    updateFilter,
    clearFilters,
    toggleSort
  };
};
