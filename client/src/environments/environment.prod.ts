export const environment = {
  production: true,
  get apiBasedUrl() {
    return `api/`;
  },
  get hubUrl() {
    return `hubs/`;
  }
};
