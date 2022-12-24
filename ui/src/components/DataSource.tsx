import React from "react";

interface Props {
  dataSource(): Promise<any>;
}

interface State {
  isLoading: boolean;
  data: any;
}

class DataSource extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { isLoading: true, data: undefined };
  }

  componentDidMount(): void {
    this.refreshData(this.props.dataSource());
  }

  refreshData = async (promise: Promise<any>, showLoader = true) => {
    if (showLoader) this.setState({ isLoading: true });
    this.setState({ isLoading: false, data: await promise });
  };

  render = () => {
    if (this.state.isLoading) {
      return <></>;
    }
    if (typeof this.props.children === "function") {
      return <>{(this.props.children as any)({ data: this.state.data, ...this.props })}</>;
    }

    return (
      <>
        {React.Children.map(this.props.children, (child) => {
          if (!React.isValidElement(child)) {
            return;
          }
          return <child.type {...child.props} data={this.state.data} />;
        })}
      </>
    );
  };
}

export default DataSource;
