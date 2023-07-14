export const environment = {
  production: false,
  protocol:"https", // http, // in case no https
  port: "7192", // 5252, // in case no https
  get apiBasedUrl() {
    return `${this.protocol}://localhost:${this.port}/api/`;
  }
};
