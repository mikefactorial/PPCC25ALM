import * as React from 'react';
import './JsonViewer.css';

export interface IJsonViewerProps {
  jsonString?: string;
}

interface IJsonViewerState {
  error?: string;
  expandedPaths: Set<string>;
}

type JsonValue = string | number | boolean | null | JsonObject | JsonArray;
interface JsonObject { [key: string]: JsonValue }
type JsonArray = JsonValue[];

export class JsonViewer extends React.Component<IJsonViewerProps, IJsonViewerState> {
  constructor(props: IJsonViewerProps) {
    super(props);
    this.state = {
      error: undefined,
      expandedPaths: new Set<string>()
    };
  }

  private parseJson(jsonString?: string): JsonValue | null {
    if (!jsonString || jsonString.trim() === '') {
      return null;
    }

    try {
      return JSON.parse(jsonString) as JsonValue;
    } catch (error) {
      this.setState({ error: `Invalid JSON: ${error instanceof Error ? error.message : String(error)}` });
      return null;
    }
  }

  private togglePath = (path: string) => {
    const expandedPaths = new Set(this.state.expandedPaths);
    if (expandedPaths.has(path)) {
      expandedPaths.delete(path);
    } else {
      expandedPaths.add(path);
    }
    this.setState({ expandedPaths });
  };

  private isExpanded(path: string): boolean {
    return this.state.expandedPaths.has(path);
  }

  private renderValue(value: JsonValue, key: string, path: string, isLast: boolean): React.ReactNode {
    if (value === null) {
      return (
        <div className="json-line" key={path}>
          <span className="json-key">&quot;{key}&quot;</span>
          <span className="json-colon">: </span>
          <span className="json-null">null</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    if (value === undefined) {
      return (
        <div className="json-line" key={path}>
          <span className="json-key">&quot;{key}&quot;</span>
          <span className="json-colon">: </span>
          <span className="json-undefined">undefined</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    const type = typeof value;

    if (type === 'string') {
      return (
        <div className="json-line" key={path}>
          <span className="json-key">&quot;{key}&quot;</span>
          <span className="json-colon">: </span>
          <span className="json-string">&quot;{value}&quot;</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    if (type === 'number') {
      return (
        <div className="json-line" key={path}>
          <span className="json-key">&quot;{key}&quot;</span>
          <span className="json-colon">: </span>
          <span className="json-number">{value}</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    if (type === 'boolean') {
      return (
        <div className="json-line" key={path}>
          <span className="json-key">&quot;{key}&quot;</span>
          <span className="json-colon">: </span>
          <span className="json-boolean">{value ? 'true' : 'false'}</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    if (Array.isArray(value)) {
      return this.renderArray(value, key, path, isLast);
    }

    if (type === 'object') {
      return this.renderObject(value as JsonObject, key, path, isLast);
    }

    return null;
  }

  private renderArray(array: JsonArray, key: string, path: string, isLast: boolean): React.ReactNode {
    const isExpanded = this.isExpanded(path);
    const isEmpty = array.length === 0;

    if (isEmpty) {
      return (
        <div className="json-line" key={path}>
          <span className="json-key">&quot;{key}&quot;</span>
          <span className="json-colon">: </span>
          <span className="json-bracket">[]</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    return (
      <div key={path}>
        <div className="json-line">
          <span
            className="json-expand-icon"
            onClick={() => this.togglePath(path)}
            role="button"
            tabIndex={0}
          >
            {isExpanded ? '▼' : '▶'}
          </span>
          <span className="json-key">&quot;{key}&quot;</span>
          <span className="json-colon">: </span>
          <span className="json-bracket">[</span>
          {!isExpanded && (
            <>
              <span className="json-preview"> {array.length} items </span>
              <span className="json-bracket">]</span>
            </>
          )}
        </div>
        {isExpanded && (
          <div className="json-nested">
            {array.map((item, index) => (
              this.renderArrayItem(item, index, `${path}[${index}]`, index === array.length - 1)
            ))}
            <div className="json-line">
              <span className="json-bracket">]</span>
              {!isLast && <span className="json-comma">,</span>}
            </div>
          </div>
        )}
      </div>
    );
  }

  private renderArrayItem(value: JsonValue, index: number, path: string, isLast: boolean): React.ReactNode {
    if (value === null) {
      return (
        <div className="json-line" key={path}>
          <span className="json-null">null</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    const type = typeof value;

    if (type === 'string') {
      return (
        <div className="json-line" key={path}>
          <span className="json-string">&quot;{value}&quot;</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    if (type === 'number') {
      return (
        <div className="json-line" key={path}>
          <span className="json-number">{value}</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    if (type === 'boolean') {
      return (
        <div className="json-line" key={path}>
          <span className="json-boolean">{value ? 'true' : 'false'}</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    if (Array.isArray(value)) {
      const isExpanded = this.isExpanded(path);
      const isEmpty = value.length === 0;

      if (isEmpty) {
        return (
          <div className="json-line" key={path}>
            <span className="json-bracket">[]</span>
            {!isLast && <span className="json-comma">,</span>}
          </div>
        );
      }

      return (
        <div key={path}>
          <div className="json-line">
            <span
              className="json-expand-icon"
              onClick={() => this.togglePath(path)}
              role="button"
              tabIndex={0}
            >
              {isExpanded ? '▼' : '▶'}
            </span>
            <span className="json-bracket">[</span>
            {!isExpanded && (
              <>
                <span className="json-preview"> {value.length} items </span>
                <span className="json-bracket">]</span>
              </>
            )}
          </div>
          {isExpanded && (
            <div className="json-nested">
              {value.map((item, idx) => (
                this.renderArrayItem(item, idx, `${path}[${idx}]`, idx === value.length - 1)
              ))}
              <div className="json-line">
                <span className="json-bracket">]</span>
                {!isLast && <span className="json-comma">,</span>}
              </div>
            </div>
          )}
        </div>
      );
    }

    if (type === 'object' && value !== null) {
      const objValue = value as JsonObject;
      const keys = Object.keys(objValue);
      const isExpanded = this.isExpanded(path);
      const isEmpty = keys.length === 0;

      if (isEmpty) {
        return (
          <div className="json-line" key={path}>
            <span className="json-brace">{'{}'}</span>
            {!isLast && <span className="json-comma">,</span>}
          </div>
        );
      }

      return (
        <div key={path}>
          <div className="json-line">
            <span
              className="json-expand-icon"
              onClick={() => this.togglePath(path)}
              role="button"
              tabIndex={0}
            >
              {isExpanded ? '▼' : '▶'}
            </span>
            <span className="json-brace">{'{'}</span>
            {!isExpanded && (
              <>
                <span className="json-preview"> {keys.length} keys </span>
                <span className="json-brace">{'}'}</span>
              </>
            )}
          </div>
          {isExpanded && (
            <div className="json-nested">
              {keys.map((k, idx) => (
                this.renderValue(objValue[k], k, `${path}.${k}`, idx === keys.length - 1)
              ))}
              <div className="json-line">
                <span className="json-brace">{'}'}</span>
                {!isLast && <span className="json-comma">,</span>}
              </div>
            </div>
          )}
        </div>
      );
    }

    return null;
  }

  private renderObject(obj: JsonObject, key: string, path: string, isLast: boolean): React.ReactNode {
    const keys = Object.keys(obj);
    const isExpanded = this.isExpanded(path);
    const isEmpty = keys.length === 0;

    if (isEmpty) {
      return (
        <div className="json-line" key={path}>
          <span className="json-key">&quot;{key}&quot;</span>
          <span className="json-colon">: </span>
          <span className="json-brace">{'{}'}</span>
          {!isLast && <span className="json-comma">,</span>}
        </div>
      );
    }

    return (
      <div key={path}>
        <div className="json-line">
          <span
            className="json-expand-icon"
            onClick={() => this.togglePath(path)}
            role="button"
            tabIndex={0}
          >
            {isExpanded ? '▼' : '▶'}
          </span>
          <span className="json-key">&quot;{key}&quot;</span>
          <span className="json-colon">: </span>
          <span className="json-brace">{'{'}</span>
          {!isExpanded && (
            <>
              <span className="json-preview"> {keys.length} keys </span>
              <span className="json-brace">{'}'}</span>
            </>
          )}
        </div>
        {isExpanded && (
          <div className="json-nested">
            {keys.map((k, index) => (
              this.renderValue(obj[k], k, `${path}.${k}`, index === keys.length - 1)
            ))}
            <div className="json-line">
              <span className="json-brace">{'}'}</span>
              {!isLast && <span className="json-comma">,</span>}
            </div>
          </div>
        )}
      </div>
    );
  }

  private renderRoot(data: JsonValue | null): React.ReactNode {
    if (data === null) {
      return <div className="json-line"><span className="json-null">null</span></div>;
    }

    if (Array.isArray(data)) {
      const isExpanded = this.isExpanded('root');
      const isEmpty = data.length === 0;

      if (isEmpty) {
        return <div className="json-line"><span className="json-bracket">[]</span></div>;
      }

      return (
        <div>
          <div className="json-line">
            <span
              className="json-expand-icon"
              onClick={() => this.togglePath('root')}
              role="button"
              tabIndex={0}
            >
              {isExpanded ? '▼' : '▶'}
            </span>
            <span className="json-bracket">[</span>
            {!isExpanded && (
              <>
                <span className="json-preview"> {data.length} items </span>
                <span className="json-bracket">]</span>
              </>
            )}
          </div>
          {isExpanded && (
            <div className="json-nested">
              {data.map((item, index) => (
                this.renderArrayItem(item, index, `root[${index}]`, index === data.length - 1)
              ))}
              <div className="json-line">
                <span className="json-bracket">]</span>
              </div>
            </div>
          )}
        </div>
      );
    }

    if (typeof data === 'object' && data !== null) {
      const objData = data;
      const keys = Object.keys(objData);
      const isExpanded = this.isExpanded('root');
      const isEmpty = keys.length === 0;

      if (isEmpty) {
        return <div className="json-line"><span className="json-brace">{'{}'}</span></div>;
      }

      return (
        <div>
          <div className="json-line">
            <span
              className="json-expand-icon"
              onClick={() => this.togglePath('root')}
              role="button"
              tabIndex={0}
            >
              {isExpanded ? '▼' : '▶'}
            </span>
            <span className="json-brace">{'{'}</span>
            {!isExpanded && (
              <>
                <span className="json-preview"> {keys.length} keys </span>
                <span className="json-brace">{'}'}</span>
              </>
            )}
          </div>
          {isExpanded && (
            <div className="json-nested">
              {keys.map((key, index) => (
                this.renderValue(objData[key], key, `root.${key}`, index === keys.length - 1)
              ))}
              <div className="json-line">
                <span className="json-brace">{'}'}</span>
              </div>
            </div>
          )}
        </div>
      );
    }

    // Primitive root value
    const type = typeof data;
    if (type === 'string') {
      return <div className="json-line"><span className="json-string">&quot;{data}&quot;</span></div>;
    }
    if (type === 'number') {
      return <div className="json-line"><span className="json-number">{data}</span></div>;
    }
    if (type === 'boolean') {
      return <div className="json-line"><span className="json-boolean">{data ? 'true' : 'false'}</span></div>;
    }

    return null;
  }

  public render(): React.ReactNode {
    const { jsonString } = this.props;
    const { error } = this.state;

    if (error) {
      return (
        <div className="json-viewer json-error">
          <div className="json-error-message">
            {error}
          </div>
        </div>
      );
    }

    const data = this.parseJson(jsonString);

    if (data === null && (!jsonString || jsonString.trim() === '')) {
      return (
        <div className="json-viewer json-empty">
          <div className="json-empty-message">
            No JSON data to display
          </div>
        </div>
      );
    }

    return (
      <div className="json-viewer">
        {this.renderRoot(data)}
      </div>
    );
  }
}
